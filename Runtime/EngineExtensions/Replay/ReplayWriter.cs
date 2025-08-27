using System;
using System.Collections.Generic;
using EngineExtensions.Abstractions;
using EngineExtensions.Replay.Utils;

namespace EngineExtensions.Replay {
    public sealed class ReplayWriter : IDisposable {
        readonly ReplayFile _file;
        readonly List<(int tick, long offset)> _index = new List<(int,long)>();
        readonly int _indexStride;


        public ReplayWriter(string path, ReplayMeta meta, int indexStride=20, IBlockCodec codec=null){
            _file = ReplayFile.Create(path, meta, codec);
            _indexStride = Math.Max(1, indexStride);
        }


        public void WriteBootstrap(byte[] snapshotKeyframe, int tick0=0){
            _file.Append(ReplayBlockType.BOOT, tick0, tick0, snapshotKeyframe);
            _index.Add((tick0, _file.LastAppendOffset));
        }


        public void WriteCommands(int tick, int playerId, byte[] cmdPacket){
            var payload = CmdMuxer.PackOne(playerId, cmdPacket);
            _file.Append(ReplayBlockType.CMDS, tick, tick, payload);
            TryIndex(tick);
        }


        public void WriteSnapshot(int tick, byte[] snapshotBytes){
            _file.Append(ReplayBlockType.SNAP, tick, tick, snapshotBytes);
            TryIndex(tick);
        }


        void TryIndex(int tick) {
            if ((tick % _indexStride)==0) 
                _index.Add((tick, _file.LastAppendOffset));
        }


        public void Dispose() {
            _file.FlushIndex(_index); 
            _file.Dispose();
        }
    }
}