namespace EngineExtensions.Abstractions.Utils {
    public static class VarInt {
        // Standard LEB128 unsigned
        public static void WriteVarUInt(ByteWriter w, uint value) {
            while (value >= 0x80) {
                w.AddRaw((byte)(value | 0x80));
                value >>= 7;
            }
            w.AddRaw((byte)value);
        }
        public static uint ReadVarUInt(ref ByteReader r) {
            uint result = 0; int shift = 0; byte b;
            do {
                b = r.ReadByte();
                result |= (uint)(b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return result;
        }
    }
}