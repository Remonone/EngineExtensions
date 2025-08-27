using System;
using System.Text;
using EngineExtensions.Abstractions.Utils;

namespace EngineExtensions.Core.Events.Wiring {
    public ref struct TlvReader {
        private ByteReader _r;
        public TlvReader(ReadOnlySpan<byte> data){ _r=new ByteReader(data); }
        public bool TryRead(out uint tag, out ReadOnlySpan<byte> value){
            if (_r.Eof){ tag=0; value=ReadOnlySpan<byte>.Empty; return false; }
            tag = _r.ReadVarUInt(); int len = (int)_r.ReadVarUInt(); value = _r.ReadSlice(len); return true;
        }
        // typed helpers for already-sliced value spans
        public static uint  ReadU32(ReadOnlySpan<byte> v){ var r=new ByteReader(v); return r.ReadVarUInt(); }
        public static ulong ReadU64(ReadOnlySpan<byte> v){ var r=new ByteReader(v); ulong res=0; int shift=0; while(!r.Eof){ byte b=r.ReadByte(); res |= (ulong)(b & 0x7F) << shift; shift+=7; if((b&0x80)==0) break; } return res; }
        public static int   ReadI32(ReadOnlySpan<byte> v){ var r=new ByteReader(v); return ZigZagEncoder.Decode(r.ReadVarUInt()); }
        public static long  ReadI64(ReadOnlySpan<byte> v){ var r=new ByteReader(v); ulong uv=0; int shift=0; while(!r.Eof){ byte b=r.ReadByte(); uv |= (ulong)(b & 0x7F) << shift; shift+=7; if((b&0x80)==0) break; } return ZigZagEncoder.Decode64(uv); }
        public static bool  ReadBool(ReadOnlySpan<byte> v){ return v.Length>0 && v[0]!=0; }
        public static string ReadString(ReadOnlySpan<byte> v){ return Encoding.UTF8.GetString(v); }
    }
}