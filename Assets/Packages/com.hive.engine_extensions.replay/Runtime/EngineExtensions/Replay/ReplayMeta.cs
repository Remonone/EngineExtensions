using System;
using UnityEngine;

namespace EngineExtensions.Replay {
    [Serializable]
    public sealed class ReplayMeta {
        public string Game;
        public string Build;
        public string Map;
        public string Notes;
        public int TickRate;
        public bool Authoritative;
        public int SnapshotSchema;
        public int CommandsSchema;
        public int EventsSchema;
        public string RecordedAtUtc; // ISO string for Unity JsonUtility


        public static string ToJson(ReplayMeta m) => JsonUtility.ToJson(m);
        public static ReplayMeta FromJson(string json) => JsonUtility.FromJson<ReplayMeta>(json);
    }
}