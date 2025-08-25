using EngineExtensions.Abstractions.Snapshots;

namespace EngineExtensions.Snapshots.Utils {
    public static class SnapshotMetrics {
        public static int EstimateBytes(in SnapshotFrame frame, ComponentRegistry reg) {
            var w = new SnapshotWriter(reg); return w.WriteFrame(frame).Length;
        }
    }
}