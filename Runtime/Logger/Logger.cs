using System;
using System.Collections.Generic;
using EngineExtensions.Abstractions;

namespace EngineExtensions.Logger {
    public class Logger : ILog {
        
        private readonly List<ILogSink> _sinks = new();

        public void AddSink(ILogSink sink) => _sinks.Add(sink);
        
        public void Write(LogLevel level, string message, Exception exception = null, LogContext context = null) {
            LogEntry entry = new LogEntry(level, message, exception, context);
            foreach (var sink in _sinks) {
                sink.Write(entry);
            }
        }
    }
}