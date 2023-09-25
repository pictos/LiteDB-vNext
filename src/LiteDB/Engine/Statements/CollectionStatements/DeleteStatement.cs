﻿using System.Transactions;

namespace LiteDB.Engine;

internal class DeleteStatement : IScalarStatement
{
    private readonly IDocumentStore _store;
    private readonly BsonExpression _whereExpr;

    public DeleteStatement(IDocumentStore store, BsonExpression whereExpr)
    {
        _store = store;
        _whereExpr = whereExpr;
    }

    public async ValueTask<int> ExecuteScalarAsync(IServicesFactory factory, BsonDocument parameters)
    {
        using var _pc = PERF_COUNTER(31, nameof(InsertStatement), nameof(LiteEngine));

        // dependency injection
        var masterService = factory.MasterService;
        var monitorService = factory.MonitorService;

        // initialize document store before
        _store.Initialize(masterService);

        // create a new transaction locking colID
        var transaction = await monitorService.CreateTransactionAsync(new byte[] { _store.ColID });

        // get data/index services
        var (dataService, indexService) = _store.GetServices(factory, transaction);
        var context = new PipeContext(dataService, indexService, parameters);

        var count = 0;

        // get all pk indexNode
        var allNodes = this.GetDeleteEnumerableAsync(factory, transaction, parameters);

        await foreach(var indexNodeResult in allNodes)
        {
            // get value before DeleteAsync
            var dataBlockID = indexNodeResult.DataBlockID;

            // delete all index nodes starting from PK
            indexService.DeleteAll(indexNodeResult);

            // delete document
            dataService.DeleteDocument(dataBlockID);

            count++;

            // do a safepoint after insert each document
            if (monitorService.Safepoint(transaction))
            {
                await transaction.SafepointAsync();
            }
        }

        // write all dirty pages into disk
        await transaction.CommitAsync();

        // release transaction
        monitorService.ReleaseTransaction(transaction);

        return count;
    }

    private IAsyncEnumerable<IndexNodeResult> GetDeleteEnumerableAsync(IServicesFactory factory, ITransaction transaction, BsonDocument parameters)
    {
        var (dataService, indexService) = _store.GetServices(factory, transaction);
        var context = new PipeContext(dataService, indexService, parameters);

        var indexes = _store.GetIndexes();

        var bin = (BinaryBsonExpression)_whereExpr;

        var left = bin.Left;
        var right = bin.Right;

        if (left == BsonExpression.Id)
        {

        }
        //////////

        var q = new Query { Select = BsonExpression.Id, Where = _whereExpr };





        throw new NotSupportedException();
    }
}