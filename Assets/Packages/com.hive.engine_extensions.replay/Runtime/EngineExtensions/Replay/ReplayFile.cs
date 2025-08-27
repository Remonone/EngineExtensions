using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EngineExtensions.Abstractions;
using EngineExtensions.Replay.Utils;

namespace EngineExtensions.Replay {
    public sealed class ReplayFile : IDisposable {

        const uint MAGIC = 0x4C505248;
        const ushort VERSION = 1;


        readonly FileStream _fs;
        readonly IBlockCodec _codec;


        private long _indexOffsetPos;


        public ReplayMeta Meta { get; private set; }
        public long Length => _fs.Length;
        public long LastAppendOffset { get; private set; }


        private ReplayFile(FileStream fs, IBlockCodec codec){ _fs=fs; _codec = codec ?? new NoCompressionCodec(); }


        public static ReplayFile Create(string path, ReplayMeta meta, IBlockCodec codec=null){
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            var rf = new ReplayFile(fs, codec){ Meta = meta };
            rf.WriteHeader(meta);
            return rf;
        }


        public static ReplayFile OpenRead(string path, IBlockCodec codec=null){
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var rf = new ReplayFile(fs, codec);
            rf.ReadHeader();
            return rf;
        }
        
        void WriteHeader(ReplayMeta meta){
            var bw = new BinaryWriter(_fs, Encoding.UTF8, true);
            bw.Write(MAGIC);
            bw.Write(VERSION);
            bw.Write((byte)0); // flags
            bw.Write((byte)0); // reserved
            var json = Encoding.UTF8.GetBytes(ReplayMeta.ToJson(meta) ?? "{}");
            WriteVarUInt(bw, (uint)json.Length);
            bw.Write(json);
            _indexOffsetPos = _fs.Position; // remember where to patch
            bw.Write((ulong)0); // indexOffset placeholder
            bw.Flush();
        }


        void ReadHeader(){
            var br = new BinaryReader(_fs, Encoding.UTF8, true);
            uint magic = br.ReadUInt32(); if (magic != MAGIC) throw new InvalidDataException("Invalid replay magic");
            ushort ver = br.ReadUInt16(); if (ver != VERSION) throw new InvalidDataException($"Unsupported replay version {ver}");
            br.ReadByte(); br.ReadByte(); // flags, reserved
            uint metaLen = ReadVarUInt(br);
            var metaBytes = br.ReadBytes((int)metaLen);
            Meta = ReplayMeta.FromJson(Encoding.UTF8.GetString(metaBytes));
            br.ReadUInt64();
        }


        public long Append(ReplayBlockType type, int tickStart, int tickEnd, byte[] rawPayload){
            byte[] payload = _codec.Encode(rawPayload ?? Array.Empty<byte>());
            var bw = new BinaryWriter(_fs, Encoding.UTF8, true);
            LastAppendOffset = _fs.Position;
            // write block
            bw.Write((byte)type);
            WriteVarUInt(bw, (uint)tickStart);
            WriteVarUInt(bw, (uint)tickEnd);
            WriteVarUInt(bw, (uint)payload.Length);
            bw.Write(payload);
            // crc over [type|t0|t1|len|payload]
            _fs.Flush();
            long blockStart = LastAppendOffset;
            int crcCount = (int)(_fs.Position - blockStart);
            _fs.Position = blockStart; var buf = new byte[crcCount]; _fs.Read(buf, 0, crcCount); _fs.Position = blockStart + crcCount;
            uint crc = CRC32.Compute(buf, 0, crcCount);
            bw.Write(crc);
            bw.Flush();
            return LastAppendOffset;
        }
        
        public void FlushIndex(IEnumerable<(int tick, long offset)> indexEntries){
            // materialize list to know count
            var list = new List<(int tick, long offset)>(indexEntries ?? Array.Empty<(int,long)>());
            // write index block payload: [count][ {tick,offset} * ]
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms, Encoding.UTF8, true);
            WriteVarUInt(bw, (uint)list.Count);
            foreach (var (tick, off) in list){ WriteVarUInt(bw, (uint)tick); bw.Write((ulong)off); }
            var payload = ms.ToArray();
            long indexOffset = Append(ReplayBlockType.INDEX, 0, 0, payload);
            // patch header with indexOffset
            long cur = _fs.Position;
            _fs.Position = _indexOffsetPos; new BinaryWriter(_fs).Write((ulong)indexOffset);
            _fs.Position = cur; _fs.Flush();
        }


        public (ReplayMeta meta, long indexOffset) ReadHeaderQuick(){
            _fs.Position = 0; 
            var br = new BinaryReader(_fs, Encoding.UTF8, true);
            uint magic = br.ReadUInt32(); 
            if (magic != MAGIC) 
                throw new InvalidDataException("Invalid replay magic");
            ushort ver = br.ReadUInt16(); 
            if (ver != VERSION) 
                throw new InvalidDataException($"Unsupported replay version {ver}");
            br.ReadByte(); 
            br.ReadByte();
            uint metaLen = ReadVarUInt(br); 
            var metaBytes = br.ReadBytes((int)metaLen);
            var meta = ReplayMeta.FromJson(Encoding.UTF8.GetString(metaBytes));
            long indexOffset = (long)br.ReadUInt64();
            return (meta, indexOffset);
        }
        
        public IEnumerable<(ReplayBlockType type, int t0, int t1, byte[] payload, long offset)> ReadAll(){
            _fs.Position = HeaderSize(); var br = new BinaryReader(_fs, Encoding.UTF8, true);
            while (_fs.Position < _fs.Length){
                long off = _fs.Position;
                byte type = br.ReadByte();
                int t0 = (int)ReadVarUInt(br); int t1 = (int)ReadVarUInt(br);
                int len = (int)ReadVarUInt(br); var payloadEncoded = br.ReadBytes(len);
                uint crcStored = br.ReadUInt32();
                // verify CRC against encoded payload
                uint crcCalc = ComputeBlockCrc(type, t0, t1, payloadEncoded);
                if (crcCalc != crcStored) throw new InvalidDataException($"Replay CRC mismatch at 0x{off:X}");
                // decode payload only after CRC passes
                var payload = _codec.Decode(payloadEncoded);
                yield return ((ReplayBlockType)type, t0, t1, payload, off);
            }
        }
        
        static uint ComputeBlockCrc(byte type, int t0, int t1, byte[] payload){
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms, Encoding.UTF8, true);
            bw.Write(type);
            WriteVarUInt(bw, (uint)t0); 
            WriteVarUInt(bw, (uint)t1); 
            WriteVarUInt(bw, (uint)(payload?.Length ?? 0));
            if (payload!=null) bw.Write(payload);
            var buf = ms.ToArray();
            return CRC32.Compute(buf, 0, buf.Length);
        }
        
        long HeaderSize(){
            _fs.Position = 0; 
            var br = new BinaryReader(_fs, Encoding.UTF8, true);
            br.ReadUInt32(); 
            br.ReadUInt16(); 
            br.ReadByte(); 
            br.ReadByte();
            uint metaLen = ReadVarUInt(br); 
            _fs.Position += metaLen; 
            long pos = _fs.Position + 8;
            return pos;
        }

        static void WriteVarUInt(BinaryWriter bw, uint v) {
            while (v >= 0x80) {
                bw.Write((byte)(v|0x80)); 
                v>>=7;
            } 
            bw.Write((byte)v);
        }

        static uint ReadVarUInt(BinaryReader br) {
            uint res=0; 
            int shift=0; 
            byte b;
            do {
                b=br.ReadByte(); 
                res |= (uint)(b & 0x7F) << shift; shift+=7;
            } while((b & 0x80)!=0); 
            return res;
        }

        static int VarUIntSize(uint v) {
            int n=1;
            while (v >= 0x80) {
                v>>=7; 
                n++;
            } 
            return n;
        }


        public void Dispose(){ _fs?.Dispose(); }
    }
    


public interface IBlockCodec { byte[] Encode(byte[] raw); byte[] Decode(byte[] packed); }
public sealed class NoCompressionCodec : IBlockCodec { public byte[] Encode(byte[] raw)=>raw ?? Array.Empty<byte>(); public byte[] Decode(byte[] packed)=>packed ?? Array.Empty<byte>(); }
}