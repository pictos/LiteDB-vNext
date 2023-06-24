﻿namespace LiteDB.Engine;

[AutoInterface]
internal class LimitEnumerator : ILimitEnumerator
{
    private readonly IPipelineEnumerator _enumerator;

    private readonly int _limit;

    private int _count = 0;
    private bool _eof = false;

    public LimitEnumerator(int limit, IPipelineEnumerator enumerator)
    {
        _limit = limit;
        _enumerator = enumerator;
    }

    public async ValueTask<BsonDocument?> MoveNextAsync(ITransaction transacion, IServicesFactory factory)
    {
        if (_eof) return null;

        var doc = await _enumerator.MoveNextAsync(transacion, factory);

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
