using System;

namespace EngineExtensions.Abstractions.Events {
    public interface IWireEventCodecBase {
    uint TypeId { get; }
    Type EventType { get; }
    byte[] EncodeObject(object e);
    object DecodeObject(ReadOnlySpan<byte> payload);
    }
}