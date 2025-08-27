using System;
using System.Collections.Generic;
using EngineExtensions.Abstractions.Utils;
using EngineExtensions.Core.Events.Registry;

namespace EngineExtensions.Core.Events.Utils {
    public static class EventBlock {
        public static void Write(ref ByteWriter w, WireEventRegistry reg, IReadOnlyList<object> events){
            w.WriteVarUInt((uint)(events?.Count ?? 0));
            if (events == null) return;
            foreach (var e in events){
                if (!reg.TryGetByType(e.GetType(), out var codec)) throw new InvalidOperationException($"No codec for {e.GetType().Name}");
                var payload = codec.EncodeObject(e);
                w.WriteVarUInt(codec.TypeId);
                w.WriteVarUInt((uint)payload.Length);
                w.Write(payload);
            }
        }

        public static List<object> Read(ref ByteReader r, WireEventRegistry reg){
            int count = (int)r.ReadVarUInt(); var list = new List<object>(count);
            for (int i=0;i<count;i++){
                uint typeId = r.ReadVarUInt(); int len = (int)r.ReadVarUInt(); var payload = r.ReadSlice(len);
                list.Add(reg.DecodeObject(typeId, payload));
            }
            return list;
        }
    }
}