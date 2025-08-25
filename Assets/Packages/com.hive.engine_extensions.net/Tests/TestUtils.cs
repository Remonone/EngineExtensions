using System;
using System.Collections.Generic;
using System.Linq;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Net;
using EngineExtensions.Abstractions.Telemetry;

namespace EngineExtensions.Tests {
    public sealed class LoopbackTransport : ITransport {
        public event Action<byte,int,ReadOnlyMemory<byte>> OnMessage;
        public void Dispose() {}
        public void Send(byte code, ReadOnlySpan<byte> payload, Reliability reliability, byte channel = 0) {
            var bytes = payload.ToArray();
            OnMessage?.Invoke(code, 999, bytes);
        }
    }
    public sealed class FakeClock {
        public double Now; public double Get() => Now; public void Advance(double seconds) => Now += seconds;
    }
    
    public sealed class TelemetryCollector {
        public readonly List<TelemetryMarker> Markers = new();
        public void Emit(TelemetryMarker m) => Markers.Add(m);
        public int CountByName(string name) => Markers.Count(x => x.Name == name);
    }
}