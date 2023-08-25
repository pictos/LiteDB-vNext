﻿namespace LiteDB.Engine;

internal class IndexLikeEnumerator : IPipeEnumerator
{
    private readonly Collation _collation;

    private readonly IndexDocument _indexDocument;
    private readonly BsonValue _value;
    private readonly string _startsWith;
    private readonly bool _hasMore;
    private readonly int _order;

    private bool _init = false;
    private bool _eof = false;

    private PageAddress _prev = PageAddress.Empty; // all nodes from left of first node found
    private PageAddress _next = PageAddress.Empty; // all nodes from right of first node found

    public IndexLikeEnumerator(
        BsonValue value, 
        IndexDocument indexDocument, 
        Collation collation,
        int order)
    {
        _startsWith = value.AsString.SqlLikeStartsWith(out _hasMore);
        _value = value;
        _indexDocument = indexDocument;
        _collation = collation;
        _order = order;
    }

    public PipeEmit Emit => new(true, true, false);

    public ValueTask<PipeValue> MoveNextAsync(PipeContext context)
    {
        if (_eof) return new ValueTask<PipeValue>(PipeValue.Empty);

        return _startsWith.Length > 0 ?  this.ExecuteLike(context) : this.ExecuteFullScan(context);
    }

    private async ValueTask<PipeValue> ExecuteLike(PipeContext context)
    {
        var indexService = context.IndexService;

        if(!_init)
        {
            _init = true;

            var (node, _) = await indexService.FindAsync(_indexDocument, _startsWith, true, Query.Ascending);

            if (node.IsEmpty)
            {
                _eof = true;
                return PipeValue.Empty;
            };

            // get pointer to next/prev
            _prev = node.Prev[0];
            _next = node.Next[0];
        }

        if(!_next.IsEmpty || !_prev.IsEmpty)
        {
            var (node, _) = !_next.IsEmpty ?
                await indexService.GetNodeAsync(_next) :
                await indexService.GetNodeAsync(_prev);
            
            if(node.Key.AsString.StartsWith(_startsWith, StringComparison.OrdinalIgnoreCase))
            {
                if(!_hasMore || (_hasMore && node.Key.AsString.SqlLike(_value, _collation)))
                {
                    return new PipeValue(node.IndexNodeID, node.DataBlockID);
                }
            }
        }
        return PipeValue.Empty;
    }

    private async ValueTask<PipeValue> ExecuteFullScan(PipeContext context)
    {
        var indexService = context.IndexService;

        if (_eof) return PipeValue.Empty;

        // in first run, gets head node
        if (!_init)
        {
            _init = true;

            var start = _order == Query.Ascending ? _indexDocument.HeadIndexNodeID : _indexDocument.TailIndexNodeID;

            var nodeRef = await indexService.GetNodeAsync(start);
            var node = nodeRef.Node;

            // get pointer to next at level 0
            _next = nodeRef.Node.GetNextPrev(0, _order);

            if (node.Key.AsString.SqlLike(_value, _collation))
            {
                return new PipeValue(node.IndexNodeID, node.DataBlockID);
            }
        }
        // go forward
        if (!_next.IsEmpty)
        {
            do
            {
                var nodeRef = await indexService.GetNodeAsync(_next);
                var node = nodeRef.Node;

                if (node.Key.AsString.SqlLike(_value, _collation))
                {
                    return new PipeValue(node.IndexNodeID, node.DataBlockID);
                }

                _next = nodeRef.Node.GetNextPrev(0, _order);

            } while (!_next.IsEmpty);
        }

        _eof = true;

        return PipeValue.Empty;
    }

    public void Dispose()
    {
    }
}
