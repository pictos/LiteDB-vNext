﻿namespace LiteDB.Tests.Internals.Engine;

internal class MockEnumerator : IPipeEnumerator
{
    private readonly Queue<PipeValue> _items;

    public MockEnumerator(IEnumerable<PipeValue> values)
    {
        _items = new Queue<PipeValue>(values);
    }

    public PipeEmit Emit => new(true, true);

    ValueTask<PipeValue> IPipeEnumerator.MoveNextAsync(PipeContext context)
    {
        if (_items.Count == 0) return ValueTask.FromResult(PipeValue.Empty);

        var item = _items.Dequeue();

        return ValueTask.FromResult(item);
    }

    public void Dispose()
    {
    }
}