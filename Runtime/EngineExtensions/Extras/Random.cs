using EngineExtensions.Abstractions;

namespace EngineExtensions.Extras {
    public class Random : IRandom {
        private uint _state;
        public Random(uint seed) {
            _state = seed != 0 ? seed : 2463534242u;
        }

        public uint Next() {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public int NextRange(int min, int max) {
            return min + (int)((max - min) * NextFloat());
        }

        public float NextFloat() {
            return (Next() & 0xFFFFFF) / (float)0x1000000;
        }

        public IRandom Fork(uint? seed = null) {
            return new Random(seed ?? Next());
        }
    }
}