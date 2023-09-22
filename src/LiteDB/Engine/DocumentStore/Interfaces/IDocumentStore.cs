﻿namespace LiteDB.Engine;

internal interface IDocumentStore
{
    byte ColID { get; }
    string Name { get; }

    void Initialize(IMasterService masterService);

    IReadOnlyList<IndexDocument> GetIndexes();

    (IDataService dataService, IIndexService indexService) GetServices(IServicesFactory factory, ITransaction transaction);

    IPipeEnumerator GetPipeEnumerator(BsonExpression expression);


}
