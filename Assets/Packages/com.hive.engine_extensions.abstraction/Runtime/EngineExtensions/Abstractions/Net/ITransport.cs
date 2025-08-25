using System;

namespace EngineExtensions.Abstractions.Net {
    public interface ITransport : IDisposable {
        event Action<byte,int,ReadOnlyMemory<byte>> OnMessage; // code, sender, payload
        void Send(byte code, ReadOnlySpan<byte> payload, Reliability reliability, byte channel = 0);
    }
}