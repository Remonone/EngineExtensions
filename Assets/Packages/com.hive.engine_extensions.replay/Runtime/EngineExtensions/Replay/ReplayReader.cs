using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EngineExtensions.Abstractions;
using EngineExtensions.Replay.Utils;

namespace EngineExtensions.Replay {
    public sealed class ReplayReader : IDisposable {
        readonly ReplayFile _file;
        readonly List<(int tick, long offset)> _index = new List<(int,long)>();

        public ReplayMeta Meta => _file.Meta;
        public IReadOnlyList<(int tick, long offset)> Index => _index;

        public ReplayReader(string path, IBlockCodec codec=null){
            _file = ReplayFile.OpenRead(path, codec);
            LoadIndex();
        }

        void LoadIndex(){
            var (meta, idxOff) = _file.ReadHeaderQuick();
            _index.Clear();
            if (idxOff == 0){
                // fallback: linear scan creating sparse index every 50 blocks
                int step=50; int lastTick=0; int i=0;
                foreach (var (type, t0, _t1, _payload, off) in _file.ReadAll()){
                    if (i++ % step == 0) _index.Add((t0, off)); lastTick=t0;
                }
                if (_index.Count==0) _index.Add((0, 0));
                return;
            }
            // read explicit index
            foreach (var (type, t0, t1, payload, off) in _file.ReadAll()){
                if (type != ReplayBlockType.INDEX) continue;
                var br = new BinaryReader(new MemoryStream(payload), Encoding.UTF8, false);
                uint cnt = ReadVarUInt(br);
                for (int i = 0; i < cnt; i++) {
                    int tick=(int)ReadVarUInt(br); 
                    long pos=(long)br.ReadUInt64(); 
                    _index.Add((tick, pos));
                }
            }
            if (_index.Count==0) _index.Add((0, HeaderSize()));
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
        long HeaderSize(){ // proxy to ReplayFile.HeaderSize via quick read
            var (_,_) = _file.ReadHeaderQuick(); // positions stream at after offset read
            return 0; // not used here
        }

        public (int tick, byte[] keyframe) ReadBootstrap(){
            foreach (var (type, t0, _t1, payload, _off) in _file.ReadAll())
                if (type == ReplayBlockType.BOOT) 
                    return (t0, payload);
            throw new InvalidDataException("No BOOT block in replay");
        }

        public IEnumerable<(int tick, int playerId, byte[] packet)> ReadCommandsRange(int tFrom, int tTo){
            foreach (var (type, t0, _t1, payload, _off) in _file.ReadAll()){
                if (type != ReplayBlockType.CMDS) continue;
                if (t0 < tFrom || t0 > tTo) continue;
                foreach (var (pid, p) in CmdMuxer.Unpack(payload)) 
                    yield return (t0, pid, p);
            }
        }

        public IEnumerable<(int tick, byte[] snapshot)> ReadSnapshotsRange(int tFrom, int tTo){
            foreach (var (type, t0, _t1, payload, _off) in _file.ReadAll()){
                if (type != ReplayBlockType.SNAP) continue;
                if (t0 < tFrom || t0 > tTo) continue;
                yield return (t0, payload);
            }
        }

        public void SeekToTick(int tick, out (int foundTick, long offset) pos){
            if (_index.Count == 0) {
                pos=(0,0); 
                return;
            }
            int lo=0, hi=_index.Count-1, ans=0;
            while (lo<=hi){ int mid=(lo+hi)>>1;
                if (_index[mid].tick <= tick) {
                    ans=mid; 
                    lo=mid+1;
                } else hi=mid-1;
            }
            pos = _index[ans];
        }

        public IEnumerable<(ReplayBlockType type, int tick, byte[] payload)> ReadFrom(long fileOffset){
            foreach (var (type, t0, _t1, payload, off) in _file.ReadAll()){
                if (off < fileOffset) continue;
                yield return (type, t0, payload);
            }
        }

        public void Dispose(){ _file?.Dispose(); }
    }
}