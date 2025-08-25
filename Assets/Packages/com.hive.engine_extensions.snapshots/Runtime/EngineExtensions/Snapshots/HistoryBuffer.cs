using EngineExtensions.Abstractions.Snapshots;

namespace EngineExtensions.Snapshots {
    public sealed class HistoryBuffer {
        private readonly int _capacity; private readonly System.Collections.Generic.Dictionary<int, SnapshotFrame> _frames = new();
        public HistoryBuffer(int capacity) { _capacity = capacity; }
        public void Put(in SnapshotFrame f) { _frames[f.Tick] = f; Trim(); }
        public bool TryGet(int tick, out SnapshotFrame f) => _frames.TryGetValue(tick, out f);

        private void Trim() {
            if (_frames.Count <= _capacity) return; 
            int min = int.MaxValue; 
            foreach (var k in _frames.Keys) 
                if (k<min) 
                    min=k; 
            _frames.Remove(min);
        }
    }
}