using UnityEngine;

namespace EngineExtensions.Net {
    [CreateAssetMenu(fileName = "NetConfig", menuName = "Hive/Net/Config", order = 0)]
    public sealed class NetConfig : ScriptableObject {
        [Header("Tick & Rates")]
        public int TickRate = 60;
        public int SendRate = 20;
        public int KeyframeInterval = 3;
        public int InterpDelayTicks = 3;
        [Header("Channels & Codes")]
        public byte CommandsChannel = 0;
        public byte SnapshotsChannel = 1;
        public byte CommandCode = 1;
        public byte SnapshotCode = 2;
    }
}