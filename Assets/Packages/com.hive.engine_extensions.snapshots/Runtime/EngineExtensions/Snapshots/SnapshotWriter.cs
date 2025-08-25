using System;
using System.Collections.Generic;
using EngineExtensions.Abstractions.Snapshots;
using EngineExtensions.Abstractions.Utils;

namespace EngineExtensions.Snapshots {
    public sealed class SnapshotWriter {
        private readonly ComponentRegistry _reg;
        // Baseline: entityId -> (typeId -> canonical bytes)
        private readonly Dictionary<ulong, Dictionary<ushort, byte[]>> _last = new();

        public SnapshotWriter(ComponentRegistry registry){ _reg = registry ?? throw new ArgumentNullException(nameof(registry)); }

        public byte[] WriteFrame(in SnapshotFrame frame) {
            var w = new ByteWriter(512);
            w.WriteByte(SnapshotSchema.Version);
            w.WriteByte((byte)(frame.Keyframe ? 1 : 0));
            w.WriteVarUInt((uint)frame.Tick);

            if (frame.Keyframe) {
                w.WriteVarUInt((uint)frame.Entities.Length);
                foreach (var es in frame.Entities) WriteEntityKey(ref w, es);
            } else {
                // delta: only entities with any change
                var changedEntities = new List<EntityState>();
                foreach (var cur in frame.Entities) 
                    if (EntityChanged(cur)) changedEntities.Add(cur);
                w.WriteVarUInt((uint)changedEntities.Count);
                foreach (var es in changedEntities) 
                    WriteEntityDelta(ref w, es);
            }

            // Update baseline to current
            _last.Clear();
            foreach (var es in frame.Entities) _last[es.EntityId] = ToMap(es.Components);

            return w.ToArray();
        }

        private static Dictionary<ushort, byte[]> ToMap(List<ComponentState> list){
            var map = new Dictionary<ushort, byte[]>(list.Count);
            foreach (var c in list) map[c.TypeId] = c.Bytes;
            return map;
        }

        private void WriteEntityKey(ref ByteWriter w, EntityState es){
            w.WriteVarUInt((uint)es.EntityId); w.WriteVarUInt(es.ArchetypeId); w.WriteVarUInt((uint)es.Components.Count);
            foreach (var c in es.Components){
                var codec = _reg.Get(c.TypeId);
                w.WriteVarUInt(c.TypeId);
                codec.WriteKey(ref w, c.Bytes);
            }
        }

        private bool EntityChanged(EntityState cur){
            if (!_last.TryGetValue(cur.EntityId, out var prevMap)) return true;
            var curMap = ToMap(cur.Components);
            foreach (var kv in prevMap) if (!curMap.ContainsKey(kv.Key)) return true;
                foreach (var kv in curMap){
                if (!prevMap.TryGetValue(kv.Key, out var prevBytes)) return true;
                var codec = _reg.Get(kv.Key);
                if (!codec.AreEqual(prevBytes, kv.Value)) return true;
            }
            return false;
        }

        private void WriteEntityDelta(ref ByteWriter w, EntityState cur){
            w.WriteVarUInt((uint)cur.EntityId); w.WriteVarUInt(cur.ArchetypeId);
            var curMap = ToMap(cur.Components);
            _last.TryGetValue(cur.EntityId, out var prevMap); prevMap ??= new Dictionary<ushort, byte[]>();

            // Compute removed & changed
            var removed = new List<ushort>();
            foreach (var kv in prevMap) 
                if (!curMap.ContainsKey(kv.Key)) removed.Add(kv.Key);

            var changed = new List<(ushort type, byte[] prev, byte[] cur)>();
            foreach (var kv in curMap) {
                prevMap.TryGetValue(kv.Key, out var prev); 
                var codec = _reg.Get(kv.Key); 
                if (prev==null || !codec.AreEqual(prev, kv.Value)) 
                    changed.Add((kv.Key, prev ?? Array.Empty<byte>(), kv.Value));
            }

            w.WriteVarUInt((uint)removed.Count);
            foreach (var t in removed) w.WriteVarUInt(t);

            w.WriteVarUInt((uint)changed.Count);
            foreach (var ch in changed) {
                var codec = _reg.Get(ch.type); 
                w.WriteVarUInt(ch.type); 
                codec.WriteDelta(ref w, ch.prev, ch.cur);
            }
        }
    }
}