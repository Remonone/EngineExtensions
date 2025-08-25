using System;

namespace EngineExtensions.Abstractions {

    public record LogContext {
        public int? Tick = null;
        public int? EntityId = null;
        public string Category = "";
    }
    
    public interface ILog {
        void Write(LogLevel level, string message, Exception ex = null, LogContext? context = null);
    }
}
