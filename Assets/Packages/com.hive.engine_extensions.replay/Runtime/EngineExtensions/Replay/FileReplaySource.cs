using System.Collections.Generic;

namespace EngineExtensions.Replay {
    public sealed class FileReplaySource {
        readonly ReplayReader _reader;
        readonly Dictionary<int, List<(int playerId, byte[] packet)>> _cmdCache = new();
        readonly Dictionary<int, byte[]> _snapCache = new();

        public FileReplaySource(ReplayReader reader){ _reader = reader; }

        public bool TryGetCommands(int tick, List<(int playerId, byte[] packet)> dst){
            if (_cmdCache.TryGetValue(tick, out var list)) {
                dst.AddRange(list); 
                return list.Count>0;
            }
            var any=false; 
            var tmp = new List<(int, byte[])>();
            foreach (var (t, pid, p) in _reader.ReadCommandsRange(tick, tick)) {
                tmp.Add((pid,p)); 
                any=true;
            }
            _cmdCache[tick]=tmp; 
            if (any) 
                dst.AddRange(tmp); 
            return any;
        }

        public bool TryGetSnapshot(int tick, out byte[] bytes){
            if (_snapCache.TryGetValue(tick, out bytes)) 
                return bytes!=null;
            foreach (var (t, snap) in _reader.ReadSnapshotsRange(tick, tick)) {
                _snapCache[tick]=snap; 
                bytes=snap; 
                return true;
            }
            _snapCache[tick]=null; 
            bytes=null; 
            return false;
        }
    }
}