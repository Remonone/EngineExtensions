using System;

namespace EngineExtensions.Abstractions.Utils {
    public ref struct ByteReader {
        private ReadOnlySpan<byte> _data;
        private int _pos;
        public ByteReader(ReadOnlySpan<byte> data) { _data = data; _pos = 0; }
        public int Position => _pos;
        public byte ReadByte() => _data[_pos++];
        public ReadOnlySpan<byte> ReadSlice(int len) { var s = _data.Slice(_pos, len); _pos += len; return s; }
        public uint ReadVarUInt() => VarInt.ReadVarUInt(ref this);
        public bool Eof => _pos >= _data.Length;
    }
}