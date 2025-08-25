using EngineExtensions.Abstractions.Snapshots;
using EngineExtensions.Snapshots.Utils;

namespace EngineExtensions.Snapshots {
    public sealed class Reconciler {
        private readonly SnapshotReader _reader;
        private int _authoritativeTick = -1;
        public Reconciler(ComponentRegistry reg){ _reader = new SnapshotReader(reg); }

        public void ReconcileTo(byte[] authoritativeBytes, IWorldApplier world, ICommandSource source, ICommandReplayer replayer) {
            var frame = _reader.Read(authoritativeBytes);
            frame.ApplyToWorld(world);
            if (_authoritativeTick >= 0) {
                foreach (var (_, cmd) in source.CommandsSince(frame.Tick)) replayer.Apply(cmd);
            }
            _authoritativeTick = frame.Tick;
        }
    }
}