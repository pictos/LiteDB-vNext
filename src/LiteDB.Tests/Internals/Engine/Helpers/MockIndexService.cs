﻿namespace LiteDB.Tests.Internals.Engine;

internal class MockIndexService : IIndexService
{
    private List<(PageAddress indexNodeID, BsonValue key, PageAddress dataBlockID, PageAddress prev, PageAddress next)> _values = new()
    {
        new (new PageAddress(0, 0), BsonValue.MinValue, PageAddress.Empty, PageAddress.Empty, new PageAddress(2, 0)),

        new (new PageAddress(1, 0), 245, new PageAddress(1001, 0), new PageAddress(5, 0), new PageAddress(4, 0)),
        new (new PageAddress(2, 0), 12, new PageAddress(1002, 0), new PageAddress(0, 0), new PageAddress(5, 0)),
        new (new PageAddress(3, 0), 1024, new PageAddress(1003, 0), new PageAddress(4, 0), new PageAddress(999, 0)),
        new (new PageAddress(4, 0), 256, new PageAddress(1004, 0), new PageAddress(1, 0), new PageAddress(3, 0)),
        new (new PageAddress(5, 0), 36, new PageAddress(1005, 0), new PageAddress(2, 0), new PageAddress(1, 0)),


        new (new PageAddress(999, 0), BsonValue.MaxValue, PageAddress.Empty, new PageAddress(), PageAddress.Empty)
    };

    private readonly PageBuffer _page = new PageBuffer(0);

    public PageAddress Head => _values.First().indexNodeID;

    public PageAddress Tail => _values.Last().indexNodeID;

    public ValueTask<IndexNodeResult> AddNodeAsync(byte colID, IndexDocument index, BsonValue key, PageAddress dataBlock, IndexNodeResult last)
    {
        throw new NotImplementedException();
    }

    public ValueTask<(IndexNode head, IndexNode tail)> CreateHeadTailNodesAsync(byte colID)
    {
        throw new NotImplementedException();
    }

    public ValueTask<IndexNodeResult> FindAsync(IndexDocument index, BsonValue key, bool sibling, int order)
    {
        var data = _values.FirstOrDefault(x => x.key == key);

        var node = new IndexNode(_page, data.indexNodeID, 0, 1, data.key, data.dataBlockID);

        node.SetNext(_page, 0, data.next);
        node.SetPrev(_page, 0, data.prev);

        var result = new IndexNodeResult(node, _page);

        return new ValueTask<IndexNodeResult>(result);
    }

    public int Flip()
    {
        throw new NotImplementedException();
    }

    public ValueTask<IndexNodeResult> GetNodeAsync(PageAddress indexNodeID)
    {
        var data = _values.First(x => x.indexNodeID == indexNodeID);

        var node = new IndexNode(_page, data.indexNodeID, 0, 1, data.key, data.dataBlockID);

        node.SetNext(_page, 0, data.next);
        node.SetPrev(_page, 0, data.prev);

        var result = new IndexNodeResult(node, _page);

        return new ValueTask<IndexNodeResult>(result);
    }

    public ValueTask DeleteAllAsync(IndexNodeResult nodeResult)
    {
        throw new NotImplementedException();
    }
}