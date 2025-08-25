using System;

namespace EngineExtensions.Abstractions.Telemetry {
    public interface ITelemetrySource {
        event Action<TelemetryMarker> OnTelemetry;
    }
}