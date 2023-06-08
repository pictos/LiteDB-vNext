﻿namespace LiteDB.Engine;

public partial class LiteEngine : ILiteEngine
{
    public async Task<int> CheckpointAsync()
    {
        if (_factory.State == EngineState.Open) throw ERR("must be closed");

        var lockService = _factory.LockService;
        var diskService = _factory.DiskService;
        var logService = _factory.LogService;

        // checkpoint require exclusive lock (no readers/writers)
        await lockService.EnterExclusiveAsync();

        // at this point, there is no open transaction
        // all pages in cache are ShareCounter = 0

        var result = await logService.CheckpointAsync(diskService, null);

        // release exclusive
        lockService.ExitExclusive();

        return result;
    }
}