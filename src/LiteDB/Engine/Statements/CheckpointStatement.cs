﻿namespace LiteDB.Engine;

internal class CheckpointStatement : IScalarStatement
{
    public CheckpointStatement()
    {
    }

    public async ValueTask<int> ExecuteScalarAsync(IServicesFactory factory, BsonDocument parameters)
    {
        using var _pc = PERF_COUNTER(37, nameof(ExecuteScalarAsync), nameof(CheckpointStatement));

        var lockService = factory.LockService;
        var logService = factory.LogService;

        // checkpoint require exclusive lock (no readers/writers)
        await lockService.EnterExclusiveAsync();

        // do checkpoint and returns how many pages was overrided
        var result = await logService.CheckpointAsync(false, /* true*/ false);

        // release exclusive
        lockService.ExitExclusive();

        return result;
    }
}
