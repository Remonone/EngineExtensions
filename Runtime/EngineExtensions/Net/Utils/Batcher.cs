using System;
using System.Collections.Generic;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Net;
using EngineExtensions.Abstractions.Telemetry;
using EngineExtensions.Abstractions.Utils;

namespace EngineExtensions.Net.Utils {
    internal sealed class Batcher : IDisposable {
        private readonly byte _eventCode;
        private readonly byte _channel;
        private readonly Reliability _rel;
        private readonly ITransport _transport; 
        private readonly Action<TelemetryMarker> _emit;
        private readonly List<byte[]> _segments = new(32);
        private double _nextFlushAt; private readonly double _intervalSec; private readonly Func<double> _now;
        private const byte B0 = 0x42, B1 = 0x54; // 'B''T'

        public Batcher(ITransport t, byte code, byte channel, Reliability rel, double intervalSec, Func<double> now, Action<TelemetryMarker> emit){ 
            _transport=t; _eventCode=code; _channel=channel; _rel=rel; _intervalSec=intervalSec; _now=now; _emit=emit; _nextFlushAt = now();
        }

        public void Enqueue(byte[] segment) {
            _segments.Add(segment);
        }

        public void Pump() {
            if (_segments.Count==0) return; 
            if (_now() < _nextFlushAt) return; 
            Flush();
        }
        public void Flush(){ 
            if (_segments.Count==0) return; var startTs = TelemetryMarker.NowMicros();
            var w = new ByteWriter(4 + _segments.Count*8);
            w.WriteByte(B0); w.WriteByte(B1);
            w.WriteVarUInt((uint)_segments.Count);
            ulong total=0; foreach (var s in _segments){ w.WriteVarUInt((uint)s.Length); w.Write(s); total += (ulong)s.Length; }
            var arr = w.ToArray(); _transport.Send(_eventCode, arr, _rel, _channel);
            _emit?.Invoke(new TelemetryMarker(TelemetryKind.EVENT, "net.send_batch", startTs, 0, null,
            new[]{ new TelemetryTag("code", _eventCode.ToString()) },
            new[]{ new TelemetryField("count", _segments.Count, "msgs"), new TelemetryField("bytes", arr.Length, "bytes") }));
            _segments.Clear(); _nextFlushAt = _now() + _intervalSec; 
        }
        
        public static bool TryUnpackBatch(ReadOnlyMemory<byte> payload, out int count, out ByteReader reader){
            var span = payload.Span; reader = new ByteReader(span);
            if (span.Length >= 2 && span[0]==B0 && span[1]==B1){ reader.ReadByte(); reader.ReadByte(); count=(int)reader.ReadVarUInt(); return true; }
            count = 0; return false;
        }
        public static ReadOnlySpan<byte> ReadSegment(ref ByteReader r){ var len = (int)r.ReadVarUInt(); return r.ReadSlice(len); }
        public void Dispose() { _segments.Clear(); }
    }
}