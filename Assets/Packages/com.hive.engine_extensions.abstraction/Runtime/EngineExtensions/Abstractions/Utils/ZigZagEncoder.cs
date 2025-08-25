namespace EngineExtensions.Abstractions.Utils {
    public class ZigZagEncoder {
        public static uint Encode(int v) {
            unchecked {
                return (uint)((v << 1) ^ (v >> 31));
            }
        }

        public static int Decode(uint v) {
            unchecked {
                return (int)((v >> 1) ^ (~(int)(v & 1) + 1));
            }
        }
    }
}