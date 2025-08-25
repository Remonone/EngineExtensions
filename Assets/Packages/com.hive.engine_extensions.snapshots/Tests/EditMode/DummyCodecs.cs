using System;
using EngineExtensions.Abstractions.Snapshots;
using EngineExtensions.Abstractions.Utils;

namespace EngineExtensions.Tests {
    public sealed class DummyCodecs : IComponentCodec {
        public const ushort Type = 1; public ushort TypeId => Type;

        public static byte[] CanonicalState(int x, int y){
            var w = new ByteWriter(8);
            w.WriteVarUInt(ZigZagEncoder.Encode(x)); 
            w.WriteVarUInt(ZigZagEncoder.Encode(y)); 
            return w.ToArray();
        }

        public static (int x, int y) Parse(ReadOnlySpan<byte> state) {
            var r=new ByteReader(state); 
            var x=(int)ZigZagEncoder.Decode(r.ReadVarUInt()); 
            var y=(int)ZigZagEncoder.Decode(r.ReadVarUInt()); 
            return (x,y);
        }

        public bool AreEqual(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b) => a.SequenceEqual(b.ToArray());

        public void WriteKey(ref ByteWriter w, ReadOnlySpan<byte> state) {
            var (x,y) = Parse(state); 
            w.WriteVarUInt(ZigZagEncoder.Encode(x)); 
            w.WriteVarUInt(ZigZagEncoder.Encode(y));
        }

        public void WriteDelta(ref ByteWriter w, ReadOnlySpan<byte> prev, ReadOnlySpan<byte> cur){
            var (px,py) = prev.Length>0 ? Parse(prev) : (0,0);
            var (cx,cy) = Parse(cur);
            byte mask=0; 
            if (cx!=px) mask|=1; 
            if (cy!=py) mask|=2; 
            w.WriteByte(mask);
            if ((mask&1)!=0) w.WriteVarUInt(ZigZagEncoder.Encode(cx));
            if ((mask&2)!=0) w.WriteVarUInt(ZigZagEncoder.Encode(cy));
        }

        public byte[] ReadKey(ref ByteReader r) {
            var x=(int)ZigZagEncoder.Decode(r.ReadVarUInt()); 
            var y=(int)ZigZagEncoder.Decode(r.ReadVarUInt()); return CanonicalState(x,y);
        }

        public byte[] ApplyDelta(ReadOnlySpan<byte> prev, ref ByteReader r) {
            var (x,y)=prev.Length>0?Parse(prev):(0,0); 
            byte m=r.ReadByte(); 
            if((m&1)!=0) 
                x=(int)ZigZagEncoder.Decode(r.ReadVarUInt()); 
            if((m&2)!=0) 
                y=(int)ZigZagEncoder.Decode(r.ReadVarUInt()); 
            return CanonicalState(x,y);
        }
    }
    
    public sealed class DummyHealthCodec : IComponentCodec {
        public const ushort Type = 2; public ushort TypeId => Type;

        public static byte[] Canonical(uint hp){ var w=new ByteWriter(4); w.WriteVarUInt(hp); return w.ToArray(); }
        public static uint Parse(ReadOnlySpan<byte> st){ var r=new ByteReader(st); return r.ReadVarUInt(); }

        public bool AreEqual(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b) => a.SequenceEqual(b.ToArray());
        public void WriteKey(ref ByteWriter w, ReadOnlySpan<byte> state){ var hp = Parse(state); w.WriteVarUInt(hp); }
        public void WriteDelta(ref ByteWriter w, ReadOnlySpan<byte> prev, ReadOnlySpan<byte> cur){ var hp = Parse(cur); w.WriteVarUInt(hp); }
        public byte[] ReadKey(ref ByteReader r){ var hp=r.ReadVarUInt(); return Canonical(hp); }
        public byte[] ApplyDelta(ReadOnlySpan<byte> prev, ref ByteReader r){ var hp=r.ReadVarUInt(); return Canonical(hp); }
    }
}