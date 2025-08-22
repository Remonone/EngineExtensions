namespace EngineExtensions.Abstractions {
    
    public enum LogLevel { TRACE, DEBUG, INFO, WARNING, ERROR, FATAL }

    public record LogContext {
        public int? Tick = null;
        public int? EntityId = null;
        public string Category = "";
    }
    
    public interface ILog {
        void Write(LogLevel level, string message, LogContext? context = null);
    }
}
