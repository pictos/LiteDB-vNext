﻿namespace LiteDB.Engine;

[AutoInterface(typeof(IPipeEnumerator))]
internal class LimitEnumerator : ILimitEnumerator
{
    private readonly IPipeEnumerator _enumerator;

    private readonly int _limit;

    private int _count = 0;
    private bool _eof = false;

    public LimitEnumerator(int limit, IPipeEnumerator enumerator)
    {
        _limit = limit;
        _enumerator = enumerator;
    }

    public async ValueTask<BsonDocument?> MoveNextAsync(IDataService dataService, IIndexService indexService)
    {
        if (_eof || _limit == int.MaxValue) return null; // by-pass when limit is not used

        var doc = await _enumerator.MoveNextAsync(dataService, indexService);

        if (doc is null)
        {
            _eof = true;
            return null;
        }

        _count++;

        if (_count >= _limit)
        {
            _eof = true;
        }

        return doc;
    }
}
