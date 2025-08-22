using System.IO;
using UnityEngine;

namespace EngineExtensions.Logger.Sinks {
    public sealed class JsonFileSink : ILogSink {
        private readonly string _path;
        public JsonFileSink(string fileName = null){
            var dir = Path.Combine(Application.persistentDataPath, "logs");
            Directory.CreateDirectory(dir);
            _path = Path.Combine(dir, fileName ?? $"log_{System.DateTime.UtcNow:yyyyMMdd_HHmmss}.jsonl");
        }
        public void Write(LogEntry e) {
            var json = JsonUtility.ToJson(new Ser(e));
            File.AppendAllText(_path, json + "\n");
        }

        [System.Serializable]
        private struct Ser {
            public string Level, Message, Category; 
            public int? Tick; 
            public long? EntityId;

            public Ser(LogEntry e) {
                Level=e.Level.ToString(); 
                Message=e.Message; 
                Category=e.Context?.Category ?? string.Empty; 
                Tick=e.Context?.Tick; 
                EntityId=e.Context?.EntityId;
            }
        }
    }
}