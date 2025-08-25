using System;
using System.Collections.Generic;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Net;
using EngineExtensions.Abstractions.Telemetry;
using EngineExtensions.Abstractions.Utils;
using EngineExtensions.Net.Utils;

namespace EngineExtensions.Net.Channels {
    public sealed class CommandChannel : ICommandChannel, IDisposable {
        private readonly ITransport _transport; private readonly byte _code; private readonly byte _channel; private readonly Reliability _rel;
        private readonly Dictionary<int, List<ReceivedCommand>> _incoming = new();
        private readonly Batcher _batcher; private readonly Action<TelemetryMarker> _emit; private readonly Func<double> _now;

        public CommandChannel(ITransport transport, byte eventCode, byte channel, Reliability rel, double sendIntervalSec, Func<double> now, Action<TelemetryMarker> emit) {
            _transport = transport; _code = eventCode; _channel = channel; _rel = rel; _emit = emit; _now = now;
            _batcher = new Batcher(transport, eventCode, channel, rel, sendIntervalSec, now, emit);
            _transport.OnMessage += OnMessage;
        }
        public void Dispose() { _transport.OnMessage -= OnMessage; _batcher.Dispose(); }

        public void Pump() => _batcher.Pump();
        public void Flush() => _batcher.Flush();

        public void Send(int playerId, int tick, ReadOnlySpan<byte> payload) {
            var w = new ByteWriter(32 + payload.Length);
            NetMsgHeader.Write(ref w, new NetMsgHeader(MsgType.COMMAND, tick));
            w.Write(payload); 
            var seg = w.ToArray();
            _batcher.Enqueue(seg);
            _emit?.Invoke(new TelemetryMarker(TelemetryKind.COUNTER, "net.queue", TelemetryMarker.NowMicros(), 0, null,
            new[]{ new TelemetryTag("type","command") }, 
            new[]{ new TelemetryField("bytes", seg.Length, "bytes") }));
        }

        public IEnumerable<ReceivedCommand> DequeueForTick(int tick) {
            if (_incoming.TryGetValue(tick, out var list)) {
                _incoming.Remove(tick); return list;
            }
            return Array.Empty<ReceivedCommand>();
        }

        private void OnMessage(byte code, int sender, ReadOnlyMemory<byte> payload) {
            if (code != _code) return;
            if (Batcher.TryUnpackBatch(payload, out var count, out var rBatch)) {
                for (int i = 0; i < count; i++) HandleSingle(sender, Batcher.ReadSegment(ref rBatch));
                _emit?.Invoke(new TelemetryMarker(TelemetryKind.EVENT, "net.recv_batch", TelemetryMarker.NowMicros(), 0,
                    null,
                    new[] { new TelemetryTag("type", "command"), new TelemetryTag("count", count.ToString()) },
                    Array.Empty<TelemetryField>()));
            } else {
                HandleSingle(sender, payload.Span);
            }
        }
        
        private void HandleSingle(int sender, ReadOnlySpan<byte> data) {
            var r = new ByteReader(data);
            var h = NetMsgHeader.Read(ref r);
            if (h.Type != MsgType.COMMAND) return;
            var body = data.Slice(r.Position);
            if (!_incoming.TryGetValue(h.Tick, out var list)) {
                list = new List<ReceivedCommand>(4); _incoming[h.Tick] = list;
            }
            list.Add(new ReceivedCommand(sender, h.Tick, body.ToArray()));
        }
    }
}
