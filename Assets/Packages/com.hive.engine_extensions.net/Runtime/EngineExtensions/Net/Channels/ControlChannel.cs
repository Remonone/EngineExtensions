using System;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Net;
using EngineExtensions.Abstractions.Telemetry;
using EngineExtensions.Abstractions.Utils;

namespace EngineExtensions.Net.Channels {
    public sealed class ControlChannel : IControlChannel, IDisposable {
        private readonly ITransport _transport;
        private readonly byte _code;
        private readonly byte _channel;
        private readonly Reliability _rel = Reliability.RELIABLE;
        private uint _nextSeq = 1;
        private readonly System.Collections.Generic.HashSet<uint> _awaiting = new();
        private readonly Action<TelemetryMarker> _emit;
        private readonly Func<double> _now;

        public event Action<int, ReadOnlyMemory<byte>> OnControl;
        public event Action<ControlAck> OnAck;

        public ControlChannel(ITransport t, NetConfig cfg, Func<double> now, Action<TelemetryMarker> emit){
            _transport=t; _code = (byte)(cfg.CommandCode+10); _channel = (byte)(cfg.CommandsChannel+10); // default separate channel/code
            _emit = emit; _now = now; t.OnMessage += OnMessage;
        }
        public void Dispose(){ _transport.OnMessage -= OnMessage; _awaiting.Clear(); }

        public uint Send(ReadOnlySpan<byte> payload, bool requestAck = false){
            var seq = _nextSeq++;
            var w = new ByteWriter(16 + payload.Length);
            NetMsgHeader.Write(ref w, new NetMsgHeader(MsgType.CONTROL, tick: 0, seq: seq));
            w.Write(payload);
            if (requestAck) _awaiting.Add(seq);
            _transport.Send(_code, w.ToArray(), _rel, _channel);
            _emit?.Invoke(new TelemetryMarker(TelemetryKind.EVENT, "net.control.sent", TelemetryMarker.NowMicros(), seq, null,
            new[]{ new TelemetryTag("ack", requestAck ? "true" : "false") }, new[]{ new TelemetryField("bytes", w.Length, "bytes") }));
            return seq;
        }

        public void Pump() { /* nothing to aggregate for reliable control by default */ }

        private void OnMessage(byte code, int sender, ReadOnlyMemory<byte> payload){
            if (code != _code) return;
            var r = new ByteReader(payload.Span); var h = NetMsgHeader.Read(ref r);
            if (h.Type != MsgType.CONTROL) return;
            // Heuristic: empty payload = ACK for seq=h.Seq
            if (r.Eof) {
                if (_awaiting.Remove(h.Seq)) {
                    OnAck?.Invoke(new ControlAck(h.Seq));
                    _emit?.Invoke(new TelemetryMarker(TelemetryKind.EVENT, "net.control.ack_recv", TelemetryMarker.NowMicros(), h.Seq, null,
                    null, null));
                }
                return;
            }
            // Otherwise, user control payload; if they requested ack, send empty payload back with same seq.
            var body = payload.Slice(r.Position);
            OnControl?.Invoke(sender, body);
            // Auto-ACK policy: always ack control messages that have non-empty payload
            var ackW = new ByteWriter(8); NetMsgHeader.Write(ref ackW, new NetMsgHeader(MsgType.CONTROL, 0, seq: h.Seq));
            _transport.Send(_code, ackW.ToArray(), Reliability.RELIABLE, _channel);
            _emit?.Invoke(new TelemetryMarker(TelemetryKind.EVENT, "net.control.ack_sent", TelemetryMarker.NowMicros(), h.Seq, null,
            null, null));
        }
    }
}