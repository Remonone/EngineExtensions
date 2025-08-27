using System;
using System.Text;
using EngineExtensions.Abstractions.Utils;

namespace EngineExtensions.Core.Events.Wiring {
    public static class TlvWriter {
        public static void WriteU32(ref ByteWriter w, uint tag, uint value){
            w.WriteVarUInt(tag);
            var tmp = new ByteWriter(5); tmp.WriteVarUInt(value);
            w.WriteVarUInt((uint)tmp.Length); w.Write(tmp.ToArray());
        }
        public static void WriteU64(ref ByteWriter w, uint tag, ulong value){
            w.WriteVarUInt(tag);
            // encode as varuint
            var tmp = new ByteWriter(10);
            ulong v=value; while(v>=0x80){ tmp.WriteByte((byte)(v|0x80)); v>>=7; } tmp.WriteByte((byte)v);
            w.WriteVarUInt((uint)tmp.Length); w.Write(tmp.ToArray());
        }
        public static void WriteI32(ref ByteWriter w, uint tag, int value){
            w.WriteVarUInt(tag);
            var tmp = new ByteWriter(5); tmp.WriteVarUInt(ZigZagEncoder.Encode(value));
            w.WriteVarUInt((uint)tmp.Length); w.Write(tmp.ToArray());
        }
        public static void WriteI64(ref ByteWriter w, uint tag, long value){
            w.WriteVarUInt(tag);
            var tmp = new ByteWriter(10);
            ulong enc = ZigZagEncoder.Encode64(value);
            while(enc>=0x80){ tmp.WriteByte((byte)(enc|0x80)); enc>>=7; } tmp.WriteByte((byte)enc);
            w.WriteVarUInt((uint)tmp.Length); w.Write(tmp.ToArray());
        }
        public static void WriteBool(ref ByteWriter w, uint tag, bool value){ w.WriteVarUInt(tag); w.WriteVarUInt(1); w.WriteByte(value? (byte)1 : (byte)0); }
        public static void WriteBytes(ref ByteWriter w, uint tag, ReadOnlySpan<byte> bytes){ w.WriteVarUInt(tag); w.WriteVarUInt((uint)bytes.Length); w.Write(bytes); }
        public static void WriteString(ref ByteWriter w, uint tag, string s){ var bytes = Encoding.UTF8.GetBytes(s ?? string.Empty); WriteBytes(ref w, tag, bytes); }
    }
}