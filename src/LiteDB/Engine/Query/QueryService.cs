﻿namespace LiteDB.Engine;

[AutoInterface(typeof(IDisposable))]
internal class QueryService : IQueryService
{
    // dependency injections
    private readonly IMasterService _masterService;
    private readonly IServicesFactory _factory;

    private readonly ConcurrentDictionary<Guid, Cursor> _openCursors = new();

    public QueryService(
        IMasterService masterService, 
        IServicesFactory factory)
    {
        _masterService = masterService;
        _factory = factory;
    }

    public Cursor CreateCursor(CollectionDocument collection, Query query, int readVersion)
    {
        var master = _masterService.GetMaster(false);

        var queryOptimizer = _factory.CreateQueryOptimizer(master, collection, query, readVersion);

        var enumerator = queryOptimizer.ProcessQuery();

        var cursor = new Cursor(query, enumerator, readVersion);

        _openCursors.TryAdd(cursor.CursorID, cursor);

        return cursor;
    }

    public bool TryGetCursor(Guid cursorID, out Cursor cursor) => _openCursors.TryGetValue(cursorID, out cursor);

    public async ValueTask<FetchResult> FetchAsync(Cursor cursor, IDataService dataService, IIndexService indexService, int fetchSize)
    {
        var count = 0;
        var eof = false;
        var list = new List<BsonDocument>();
        var start = Stopwatch.GetTimestamp();
        var enumerator = cursor.Enumerator;

        cursor.IsRunning = true;

        while (count < fetchSize)
        {
            var doc = await enumerator.MoveNextAsync(dataService, indexService);

            if (doc is null)
            {
                eof = true;
                break;
            }
            else
            {
                list.Add(doc);
                count++;
            }
        }

        // add computed time to run query
        cursor.ElapsedTime += DateExtensions.GetElapsedTime(start);

        // if fetch finish, remove cursor
        if (eof)
        {
            _openCursors.TryRemove(cursor.CursorID, out _);
        }

        // return all fetch results (or less if is finished)
        var from = cursor.Offset;
        var to = cursor.Offset += count; // increment Offset also
        cursor.FetchCount += count; // increment fetch count on cursor
        cursor.IsRunning = false;

        return new FetchResult
        {
            From = from,
            To = to,
            FetchCount = count,
            Eof = eof,
            Results = list
        };
    }

    public void Dispose()
    {
    }
}