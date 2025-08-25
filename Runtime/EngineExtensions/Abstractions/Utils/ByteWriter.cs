using System;
using System.Collections.Generic;

namespace EngineExtensions.Abstractions.Utils {
    public struct ByteWriter {
        private List<byte> _buf;
        public ByteWriter(int capacity) { _buf = new List<byte>(capacity); }
        public int Length => _buf.Count;
        public void WriteByte(byte v) => _buf.Add(v);
        public void Write(ReadOnlySpan<byte> data) { for (int i=0;i<data.Length;i++) _buf.Add(data[i]); }
        public void WriteVarUInt(uint v) => VarInt.WriteVarUInt(this, v);
        public byte[] ToArray() => _buf.ToArray();
        internal void AddRaw(byte b) => _buf.Add(b); // used by VarInt
    }
}