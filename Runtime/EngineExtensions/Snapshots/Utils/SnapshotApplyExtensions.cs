using System.Collections.Generic;
using EngineExtensions.Abstractions.Snapshots;

namespace EngineExtensions.Snapshots.Utils {
    public static class SnapshotApplyExtensions {
        /// Apply snapshot: for keyframe do full sync; for delta apply only changes.
        public static void ApplyToWorld(this in SnapshotFrame frame, IWorldApplier world) {
            if (frame.Keyframe) {
                var present = new HashSet<ulong>();
                foreach (var e in frame.Entities) present.Add(e.EntityId);
                foreach (var id in world.EnumerateEntities()) if (!present.Contains(id)) world.Destroy(id);
            }
            foreach (var e in frame.Entities) {
                if (!world.HasEntity(e.EntityId)) world.Create(e.EntityId, e.ArchetypeId);
                foreach (var c in e.Components) world.ApplyComponent(e.EntityId, c.TypeId, c.Bytes);
            }
        }
    }
}