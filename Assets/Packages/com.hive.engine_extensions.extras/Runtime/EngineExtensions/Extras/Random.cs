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
        
        public static int NextRange(ref uint state, int min, int max) {
            return min + (int)((max - min) * NextFloat01(ref state));
        }
        
        public static uint Next(ref uint state) {
            if (state == 0) state = 2463534242u;
            uint x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state = x;
            return x;
        }
        
        public static float NextFloat01(ref uint state)
            => (Next(ref state) >> 8) * (1f / (1 << 24));
        
        public static float NextRange(ref uint state, float min, float max) {
            uint range = (uint)(max - min);
            if (range == 0) return min;
            return min + NextFloat01(ref state) * range;
        }
    }
}