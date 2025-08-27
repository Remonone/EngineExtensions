using System;
using EngineExtensions.Abstractions.Utils;

namespace EngineExtensions.Input.Commands {
    public interface ICommandCodec {
        ushort TypeId { get; }
        bool IsEdgeTriggered => true;

        void Write(ref ByteWriter w, ReadOnlySpan<byte> canonical);
        byte[] Read(ref ByteReader r);
    }
}