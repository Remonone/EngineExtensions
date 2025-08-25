using System;
using EngineExtensions.Abstractions.Utils;

namespace EngineExtensions.Abstractions.Snapshots {
    public interface IComponentCodec {

        ushort TypeId { get; }

        bool AreEqual(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b);
        
        void WriteKey(ref ByteWriter w, ReadOnlySpan<byte> state);
        
        void WriteDelta(ref ByteWriter w, ReadOnlySpan<byte> prev, ReadOnlySpan<byte> cur);
        
        byte[] ReadKey(ref ByteReader r);
        
        byte[] ApplyDelta(ReadOnlySpan<byte> prev, ref ByteReader r);
    }
}