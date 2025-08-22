using EngineExtensions.Abstractions;
#nullable enable
namespace EngineExtensions.Logger {
    
    public record LogEntry {
        public LogLevel Level;
        public string Message;
        public LogContext? Context;
        public LogEntry(LogLevel level, string message, LogContext? context = null) {
            Level = level;
            Message = message;
            Context = context;
        }
    }
    
    public interface ILogSink {
        void Write(LogEntry entry);
    }
}