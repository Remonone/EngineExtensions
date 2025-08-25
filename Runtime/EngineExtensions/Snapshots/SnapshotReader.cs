using System;
using System.Collections.Generic;
using EngineExtensions.Abstractions.Snapshots;
using EngineExtensions.Abstractions.Utils;

namespace EngineExtensions.Snapshots {
    public sealed class SnapshotReader {
        private readonly ComponentRegistry _reg;
        private readonly Dictionary<ulong, (ushort archetype, Dictionary<ushort, byte[]> comps)> _baseline = new();

        public SnapshotReader(ComponentRegistry registry) {
            _reg = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public SnapshotFrame Read(byte[] data) {
            var r = new ByteReader(data);
            var ver = r.ReadByte(); 
            if (ver != SnapshotSchema.Version) throw new InvalidOperationException($"Unsupported snapshot schema {ver}");
            bool key = r.ReadByte()!=0;
            int tick = (int)r.ReadVarUInt();
            int count = (int)r.ReadVarUInt();

            if (key) {
                _baseline.Clear();
                for (int i=0;i<count;i++) ReadEntityKey(ref r);
            } else {
                for (int i=0;i<count;i++) ReadEntityDelta(ref r);
            }

            // materialize full state array from baseline
            var list = new List<EntityState>(_baseline.Count);
            foreach (var kv in _baseline){
                var comps = new List<ComponentState>(kv.Value.comps.Count);
                foreach (var c in kv.Value.comps) 
                    comps.Add(new ComponentState(c.Key, c.Value));
                list.Add(new EntityState(kv.Key, kv.Value.archetype, comps));
            }

            return new SnapshotFrame(tick, key, list.ToArray());
        }

        private void ReadEntityKey(ref ByteReader r){
            ulong id = r.ReadVarUInt(); ushort arch = (ushort)r.ReadVarUInt(); int compCount = (int)r.ReadVarUInt();
            var map = new Dictionary<ushort, byte[]>(compCount);
            for (int i=0;i<compCount;i++){
                ushort type = (ushort)r.ReadVarUInt(); 
                var codec = _reg.Get(type); 
                var bytes = codec.ReadKey(ref r); 
                map[type] = bytes;
            }
            _baseline[id] = (arch, map);
        }

        private void ReadEntityDelta(ref ByteReader r){
            ulong id = r.ReadVarUInt(); ushort arch = (ushort)r.ReadVarUInt();
            if (!_baseline.TryGetValue(id, out var entry)) 
                entry = (arch, new Dictionary<ushort, byte[]>());
            else entry.archetype = arch;

            int removed = (int)r.ReadVarUInt();
            for (int i = 0; i < removed; i++) {
                ushort type=(ushort)r.ReadVarUInt(); 
                entry.comps.Remove(type);
            }

            int changed = (int)r.ReadVarUInt();
            for (int i=0;i<changed;i++){
                ushort type = (ushort)r.ReadVarUInt(); var codec=_reg.Get(type);
                entry.comps.TryGetValue(type, out var prev); 
                var next = codec.ApplyDelta(prev ?? Array.Empty<byte>(), ref r); 
                entry.comps[type] = next;
            }

            _baseline[id] = entry;
        }
    }
}