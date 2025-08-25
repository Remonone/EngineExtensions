using System;

namespace EngineExtensions.Abstractions.Telemetry {
    public readonly struct TelemetryTag {
        public readonly string Key;
        public readonly string Value;
        public TelemetryTag(string key, string value) {
            Key = key;
            Value = value;
        }
    }

    public readonly struct TelemetryField {
        public readonly string Key;
        public readonly double Value;
        public readonly string Unit;
        public TelemetryField(string key, double value, string unit) {
            Key = key;
            Value = value;
            Unit = unit;
        }
    }
    
    public readonly struct TelemetryMarker {
        public readonly TelemetryKind Kind;
        public readonly string Name;
        public readonly long TimestampMicros;
        public readonly ulong SpanId;
        public readonly ulong? ParentSpanId;
        public readonly TelemetryTag[] Tags;
        public readonly TelemetryField[] Fields;

        public TelemetryMarker(
            TelemetryKind kind, string name, long timestampMicros,
            ulong spanId, ulong? parentSpanId,
            TelemetryTag[] tags, TelemetryField[] fields) {
            Kind = kind; Name = name; TimestampMicros = timestampMicros;
            SpanId = spanId; ParentSpanId = parentSpanId;
            Tags = tags ?? Array.Empty<TelemetryTag>();
            Fields = fields ?? Array.Empty<TelemetryField>();
        }

        public static long NowMicros() {
            return DateTimeOffset.UtcNow.Ticks / 10;
        }
    }

    public readonly struct JobMetrics {
        public readonly int StepsExecuted;
        public readonly int JobsCompleted;
        public readonly int QueueHigh, QueueNormal, QueueLow;
        public readonly bool BudgetExceeded;
        public JobMetrics(int steps, int completed, int qh, int qn, int ql, bool over) {
            StepsExecuted = steps; JobsCompleted = completed;
            QueueHigh=qh; QueueNormal=qn; QueueLow=ql; BudgetExceeded = over;
        }
    }
}