using EngineExtensions.Abstractions;
using UnityEngine;

namespace EngineExtensions.Logger.Sinks {
    public class UnityLogSink : ILogSink {
        public void Write(LogEntry entry) {
            var msg = entry.Context is { } c && !string.IsNullOrEmpty(c.Category) ? $"[{entry.Level}] <{c.Category}> {entry.Message} (tick={c.Tick}, ent={c.EntityId})" : $"[{entry.Level}] {entry.Message}";
            var ex = entry.Exception;
            if (ex != null) msg += $"\n{ex}";
            switch (entry.Level) {
                case LogLevel.DEBUG:
                case LogLevel.TRACE:
                case LogLevel.INFO: Debug.Log(msg); break;
                case LogLevel.WARNING: Debug.LogWarning(msg); break;
                case LogLevel.ERROR: 
                case LogLevel.FATAL: Debug.LogError(msg); break;
            }
        }
    }
}