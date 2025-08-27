using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;

namespace EngineExtensions.Extras.Utils {
    public static class Prng32
    {
        private const uint NONZERO_FALLBACK = 2463534242u;

        public static uint CreateSeed() {
            Span<byte> buf = stackalloc byte[4];
            RandomNumberGenerator.Fill(buf);
            uint s = BinaryPrimitives.ReadUInt32LittleEndian(buf);
            return s != 0 ? s : NONZERO_FALLBACK;
        }

        private static uint _ctr;
        public static uint CreateSeedFast() {
            unchecked {
                uint a = (uint)Environment.TickCount;
                uint b = (uint)Stopwatch.GetTimestamp();
                uint c = ++_ctr;
                uint z = a ^ b ^ (c * 0x9E3779B9u);
                z ^= z >> 16; z *= 0x85EBCA6Bu;
                z ^= z >> 13; z *= 0xC2B2AE35u;
                z ^= z >> 16;
                return z != 0 ? z : NONZERO_FALLBACK;
            }
        }

        public static uint SeedFromString(string s) {
            unchecked {
                uint hash = 2166136261u;
                foreach (char ch in s.ToLowerInvariant()) {
                    hash ^= (byte)ch;
                    hash *= 16777619u;
                }
                return hash != 0 ? hash : NONZERO_FALLBACK;
            }
        }
    }
}