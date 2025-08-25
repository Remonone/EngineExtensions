using EngineExtensions.Logger.Sinks;

namespace EngineExtensions.Logger {
    public class DefaultLogger : Logger {
        
        private static DefaultLogger _instance;
        
        public static DefaultLogger Instance => _instance ??= new();
        
        private DefaultLogger() {
            AddSink(new UnityLogSink());
            AddSink(new JsonFileSink());
        }
    }
}