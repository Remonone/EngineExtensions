using System;
using EngineExtensions.Abstractions;
#nullable enable
namespace EngineExtensions.Logger {
    
    public record LogEntry {
        public LogLevel Level;
        public string Message;
        public Exception? Exception;
        public LogContext? Context;
        public LogEntry(LogLevel level, string message, Exception? ex = null, LogContext? context = null) {
            Level = level;
            Message = message;
            Context = context;
            Exception = ex;
        }
    }
    
    public interface ILogSink {
        void Write(LogEntry entry);
    }
}