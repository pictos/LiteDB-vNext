﻿namespace LiteDB.Engine;

public partial class LiteEngine : ILiteEngine
{
    public async Task ShutdownAsync()
    {
        if (_factory.State != EngineState.Open) throw ERR("must be open");

        var lockService = _factory.LockService;
        var diskService = _factory.DiskService;
        var logService = _factory.LogService;
        var allocationMapService = _factory.AllocationMapService;

        // must enter in exclusive lock
        await lockService.EnterExclusiveAsync();

        // set engine state to shutdown
        _factory.State = EngineState.Shutdown;

        // do checkpoint
        await logService.CheckpointAsync(diskService, null);

        // persist all dirty amp into disk
        await allocationMapService.WriteAllChangesAsync();

        // call all dispose
        diskService.Dispose();

        // set state to close
        _factory.State = EngineState.Close;

        // release exclusive
        lockService.ExitExclusive();
    }
}