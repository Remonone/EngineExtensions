using System.Collections.Generic;
using System.IO;

namespace EngineExtensions.Replay.Utils {
    public static class CmdMuxer {
        public static byte[] PackOne(int playerId, byte[] cmd){
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms)){
                WriteVarUInt(bw, 1); // count
                WriteVarUInt(bw, (uint)playerId);
                WriteVarUInt(bw, (uint)(cmd?.Length ?? 0));
                if (cmd != null) bw.Write(cmd);
                return ms.ToArray();
            }
        }

        public static IEnumerable<(int playerId, byte[] packet)> Unpack(byte[] payload){
            using (var br = new BinaryReader(new MemoryStream(payload))){
                int cnt = (int)ReadVarUInt(br);
                for (int i=0;i<cnt;i++){
                    int pid = (int)ReadVarUInt(br); int len = (int)ReadVarUInt(br); var bytes = br.ReadBytes(len);
                    yield return (pid, bytes);
                }
            }
        }

        static void WriteVarUInt(BinaryWriter bw, uint v){ while(v>=0x80){ bw.Write((byte)(v|0x80)); v>>=7; } bw.Write((byte)v); }
        static uint ReadVarUInt(BinaryReader br){ uint res=0; int shift=0; byte b; do{ b=br.ReadByte(); res |= (uint)(b & 0x7F) << shift; shift+=7; } while((b & 0x80)!=0); return res; }
    }
}