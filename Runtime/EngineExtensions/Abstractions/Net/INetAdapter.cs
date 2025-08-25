using System;

namespace EngineExtensions.Abstractions.Net {
    public interface INetAdapter : IDisposable {
        INetTick Time { get; }
        ICommandChannel Commands { get; }
        ISnapshotChannel Snapshots { get; }
        NetStats Stats { get; }
    }
}