using System;
using System.Collections.Generic;

namespace EngineExtensions.Abstractions.Snapshots {
    public interface IWorldApplier {
        bool HasEntity(ulong id);
        void Create(ulong id, ushort archetype);
        void Destroy(ulong id);
        void ApplyComponent(ulong id, ushort typeId, ReadOnlySpan<byte> canonicalState);
        void RemoveComponent(ulong id, ushort typeId);
        IEnumerable<ulong> EnumerateEntities();
    }
}