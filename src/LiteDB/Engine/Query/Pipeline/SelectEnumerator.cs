﻿namespace LiteDB.Engine;

[AutoInterface(typeof(IPipelineEnumerator))]
internal class SelectEnumerator : ISelectEnumerator
{
    private readonly Collation _collation;
    private readonly IPipelineEnumerator _enumerator;

    private readonly BsonExpression? _selectExpression;

    private bool _eof = false;

    public SelectEnumerator(BsonExpression? selectExpression, IPipelineEnumerator enumerator, Collation collation)
    {
        _selectExpression = selectExpression;
        _enumerator = enumerator;
        _collation = collation;
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

        if (_selectExpression is null)
        {
            return doc;
        }
        else
        {
            var result = _selectExpression.Execute(doc, null, _collation);

            return result.AsDocument;
        }
    }
}
