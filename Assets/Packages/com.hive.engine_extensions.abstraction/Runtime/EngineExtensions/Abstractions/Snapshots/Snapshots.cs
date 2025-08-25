using System;
using System.Collections.Generic;

namespace EngineExtensions.Abstractions.Snapshots {
    public static class SnapshotSchema { public const byte Version = 1; }
    
    public readonly struct ComponentState {
        public readonly ushort TypeId; public readonly byte[] Bytes;
        public ComponentState(ushort typeId, byte[] bytes){ TypeId=typeId; Bytes=bytes ?? Array.Empty<byte>(); }
    }

    /// Full state of a single entity for a given frame (canonical, codec-agnostic).
    public sealed class EntityState {
        public ulong EntityId; public ushort ArchetypeId; public List<ComponentState> Components;
        public EntityState(ulong id, ushort archetype, List<ComponentState> comps){ EntityId=id; ArchetypeId=archetype; Components=comps ?? new List<ComponentState>(); }
    }

    /// Snapshot frame (tick + keyframe flag + entities). Returned by reader as full state after apply.
    public readonly struct SnapshotFrame {
        public readonly int Tick; public readonly bool Keyframe; public readonly EntityState[] Entities;
        public SnapshotFrame(int tick, bool keyframe, EntityState[] entities){ Tick=tick; Keyframe=keyframe; Entities=entities ?? Array.Empty<EntityState>(); }
    }
}