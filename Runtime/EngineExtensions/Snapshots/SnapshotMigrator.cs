using System;
using EngineExtensions.Abstractions.Snapshots;

namespace EngineExtensions.Snapshots {
    public sealed class SnapshotMigrator {
        private readonly System.Collections.Generic.Dictionary<(byte,byte), ISnapshotMigration> _m = new();
        public void Register(ISnapshotMigration mig){ _m[(mig.From, mig.To)] = mig; }
        public byte[] TryMigrate(byte[] bytes) {
            if (bytes==null || bytes.Length==0) return bytes;
            var ver = bytes[0];
            if (ver == SnapshotSchema.Version) return bytes;
            byte cur = ver; var data = bytes;
            while (cur != SnapshotSchema.Version) {
                var key = (cur, (byte)(cur+1));
                if (!_m.TryGetValue(key, out var step)) throw new InvalidOperationException($"No migration {cur}->{cur+1}");
                data = step.Migrate(data); cur = (byte)(cur+1);
            }
            return data;
        }
    }
}