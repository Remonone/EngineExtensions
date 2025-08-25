namespace EngineExtensions.Abstractions.Net {
    public readonly struct NetStats {
        public readonly int RttMs;
        public readonly int JitterMs;
        public readonly ulong BytesSent;
        public readonly ulong BytesReceived;
        public readonly uint PacketsSent;
        public readonly uint PacketsReceived;
        public NetStats(int rtt, int jitter, ulong bytesSent, ulong bytesReceived, uint packetsSent, uint packetsReceived){ RttMs=rtt; JitterMs=jitter; BytesSent=bytesSent; this.BytesReceived=bytesReceived; PacketsSent=packetsSent; PacketsReceived=packetsReceived; }
    }
}