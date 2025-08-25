using System;
using System.Collections.Generic;

namespace EngineExtensions.Abstractions.Net {
    public readonly struct ReceivedCommand {
        public readonly int SenderId;
        public readonly int Tick;
        public readonly ReadOnlyMemory<byte> Payload;
        public ReceivedCommand(int senderId, int tick, ReadOnlyMemory<byte> payload) { SenderId = senderId; Tick = tick; Payload = payload; }
    }
    public interface ICommandChannel {
        void Send(int playerId, int tick, ReadOnlySpan<byte> payload);
        IEnumerable<ReceivedCommand> DequeueForTick(int tick);
    }
}