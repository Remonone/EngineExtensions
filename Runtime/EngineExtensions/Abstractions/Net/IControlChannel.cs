using System;

namespace EngineExtensions.Abstractions.Net {
    public readonly struct ControlAck { public readonly uint Seq; public ControlAck(uint seq){ Seq=seq; } }


    public interface IControlChannel {
        /// Send a control message. If requestAck=true, receiver will reply with ACK carrying our seq.
        uint Send(ReadOnlySpan<byte> payload, bool requestAck = false);
        /// Raised when a control payload arrives (not ACK).
        event Action<int, ReadOnlyMemory<byte>> OnControl;
        /// Raised when an ACK for our previously sent control message arrives.
        event Action<ControlAck> OnAck;
    }
}