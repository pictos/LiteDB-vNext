﻿namespace LiteDB.Engine;

internal class LookupEnumerator : IPipeEnumerator
{
    private readonly IDocumentLookup _lookup;
    private readonly IPipeEnumerator _enumerator;

    private bool _eof = false;

    public LookupEnumerator(IDocumentLookup lookup, IPipeEnumerator enumerator)
    {
        _lookup = lookup;
        _enumerator = enumerator;

        if (_enumerator.Emit.DataBlockID == false) throw ERR($"Lookup pipe enumerator requires DataBlockID from last pipe");
    }

    public PipeEmit Emit => new(true, true, true);

    public PipeValue MoveNext(PipeContext context)
    {
        if (_eof) return PipeValue.Empty;

        var item = _enumerator.MoveNext(context);

        if (item.IsEmpty)
        {
            _eof = true;
            return PipeValue.Empty;
        }

        var doc = _lookup.Load(item, context);

        return new PipeValue(item.DataBlockID, doc);
    }

    public void Dispose()
    {
    }
}
