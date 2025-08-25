using System;

namespace EngineExtensions.Abstractions.Net {
    public readonly struct ReceivedSnapshot {
        public readonly int Tick; public readonly bool IsKeyframe; public readonly ReadOnlyMemory<byte> Payload;
        public ReceivedSnapshot(int tick, bool keyframe, ReadOnlyMemory<byte> payload) { Tick = tick; IsKeyframe = keyframe; Payload = payload; }
    }
    public interface ISnapshotChannel {
        void Broadcast(int tick, bool isKeyframe, ReadOnlySpan<byte> payload);
        bool TryGetLatest(out ReceivedSnapshot snapshot);
    }
}