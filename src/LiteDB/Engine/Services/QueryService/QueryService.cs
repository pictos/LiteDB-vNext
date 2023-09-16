﻿namespace LiteDB.Engine;

[AutoInterface(typeof(IDisposable))]
internal class QueryService : IQueryService
{
    // dependency injections
    private readonly IWalIndexService _walIndexService;
    private readonly IServicesFactory _factory;

    private readonly ConcurrentDictionary<Guid, Cursor> _openCursors = new();

    public QueryService(
        IWalIndexService walIndexService,
        IServicesFactory factory)
    {
        _walIndexService = walIndexService;
        _factory = factory;
    }

    public Cursor CreateCursor(CollectionDocument collection, int readVersion, Query query, BsonDocument parameters)
    {
        var queryOptimization = _factory.CreateQueryOptimization(collection, query);

        var enumerator = queryOptimization.ProcessQuery(query, parameters);

        var cursor = new Cursor(query, parameters, readVersion, enumerator);

        _openCursors.TryAdd(cursor.CursorID, cursor);

        return cursor;
    }

    public bool TryGetCursor(Guid cursorID, out Cursor cursor) => _openCursors.TryGetValue(cursorID, out cursor);

    public FetchResult FetchAsync(Cursor cursor, int fetchSize, PipeContext context)
    {
        var count = 0;
        var eof = false;
        var list = new List<BsonDocument>();
        var start = Stopwatch.GetTimestamp();
        var enumerator = cursor.Enumerator;

        // checks if readVersion still avaiable to execute (changes after checkpoint)
        if (cursor.ReadVersion < _walIndexService.MinReadVersion)
        {
            cursor.Dispose();

            _openCursors.TryRemove(cursor.CursorID, out _);

            throw ERR($"Cursor {cursor} expired");
        }

        cursor.IsRunning = true;

        var fetchSizeNext = fetchSize + 
            (cursor.NextDocument is null ? 1 : 0);

        if (cursor.NextDocument is not null)
        {
            list.Add(cursor.NextDocument);
            cursor.NextDocument = null;
            count++;
        }

        while (count < fetchSizeNext)
        {
            var item = enumerator.MoveNext(context);

            if (item.IsEmpty)
            {
                eof = true;
                break;
            }
            else if (count < fetchSize)
            {
                list.Add(item.Document!);

                count++;
            }
            else
            {
                cursor.NextDocument = item.Document;
                break;
            }
        }

        // add computed time to run query
        cursor.ElapsedTime += DateExtensions.GetElapsedTime(start);

        // if fetch finish, remove cursor
        if (eof)
        {
            cursor.Dispose();

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
            HasMore = !eof,
            Results = list
        };
    }

    public override string ToString()
    {
        return Dump.Object(new { openCursors = Dump.Array(_openCursors.Select(x => x.Key)) });
    }

    public void Dispose()
    {
        _openCursors.Clear();
    }
}
