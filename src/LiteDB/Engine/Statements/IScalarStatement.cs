﻿namespace LiteDB.Engine;

internal interface IScalarStatement : IEngineStatement
{
    ValueTask<int> ExecuteScalarAsync(IServicesFactory factory, BsonDocument parameters);
    //ValueTask<FetchResult> ExecuteFetchAsync(IServicesFactory factory, BsonDocument parameters);
}