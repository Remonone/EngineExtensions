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

        public static ulong Encode64(long v) {
            unchecked {
                return (ulong)((v << 1) ^ (v >> 63));
            }
        }

        public static long Decode64(ulong v) {
            unchecked {
                return (long)(v >> 1) ^ -(long)(v & 1UL);
            }
        }
    }
}