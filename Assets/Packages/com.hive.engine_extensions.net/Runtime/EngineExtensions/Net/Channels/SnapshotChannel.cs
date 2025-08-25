using System;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Net;
using EngineExtensions.Abstractions.Telemetry;
using EngineExtensions.Abstractions.Utils;
using EngineExtensions.Net.Utils;

namespace EngineExtensions.Net.Channels {
    public sealed class SnapshotChannel : ISnapshotChannel, IDisposable {
        private readonly ITransport _transport; 
        private readonly byte _code; 
        private readonly byte _channel; 
        private readonly Reliability _rel;
        private int _latestTick = -1; 
        private bool _latestKey; 
        private ReadOnlyMemory<byte> _latestPayload;
        private readonly Batcher _batcher; 
        private readonly Action<TelemetryMarker> _emit;
        private readonly Func<double> _now;

        public SnapshotChannel(ITransport transport, byte eventCode, byte channel, Reliability rel, double sendIntervalSec, Func<double> now, Action<TelemetryMarker> emit) {
            _transport = transport; _code = eventCode; _channel = channel; _rel = rel; _emit = emit; _now = now;
            _batcher = new Batcher(transport, eventCode, channel, rel, sendIntervalSec, now, emit);
            _transport.OnMessage += OnMessage;
        }
        public void Dispose() { _transport.OnMessage -= OnMessage; _batcher.Dispose(); }

        public void Pump() => _batcher.Pump();
        public void Flush() => _batcher.Flush();

        public void Broadcast(int tick, bool isKeyframe, ReadOnlySpan<byte> payload) {
            var w = new ByteWriter(32 + payload.Length);
            var header = new NetMsgHeader(MsgType.SNAPSHOT, tick, seq: isKeyframe ? 1u : 0u);
            NetMsgHeader.Write(ref w, in header);
            w.Write(payload);
            _batcher.Enqueue(w.ToArray());
        }

        public bool TryGetLatest(out ReceivedSnapshot snapshot) {
            if (_latestTick < 0) {
                snapshot = default;
                return false;
            }
            snapshot = new ReceivedSnapshot(_latestTick, _latestKey, _latestPayload);
            return true;
        }

        private void OnMessage(byte code, int sender, ReadOnlyMemory<byte> payload) {
            if (code != _code) return;
            if (Batcher.TryUnpackBatch(payload, out var count, out var rBatch)) {
                for (int i=0;i<count;i++) HandleSingle(Batcher.ReadSegment(ref rBatch));
                _emit?.Invoke(new TelemetryMarker(TelemetryKind.EVENT, "net.recv_batch", TelemetryMarker.NowMicros(), 0, null,
                new[]{ new TelemetryTag("type","snapshot"), new TelemetryTag("count", count.ToString()) }, Array.Empty<TelemetryField>()));
            } else {
                HandleSingle(payload.Span);
            }
        }
        private void HandleSingle(ReadOnlySpan<byte> data){
            var r = new ByteReader(data);
            var h = NetMsgHeader.Read(ref r);
            if (h.Type != MsgType.SNAPSHOT) return;
            var body = data.Slice(r.Position).ToArray();
            _latestTick = h.Tick; _latestKey = (h.Seq == 1u); _latestPayload = body;
        }
    }
}