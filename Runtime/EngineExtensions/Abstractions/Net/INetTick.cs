namespace EngineExtensions.Abstractions.Net {
    public interface INetTick {
        TickRate Rate { get; }
        int TickNow { get; }
        double TimeNowSeconds { get; }
        double TickToTime(int tick);
        int TimeToTick(double seconds);
    }
    
    public readonly struct TickRate {
        public readonly int Hz;
        public float DeltaSeconds => Hz > 0 ? 1f / Hz : 0f;
        public TickRate(int hz) { Hz = hz < 1 ? 1 : hz; }
        public override string ToString() => $"{Hz}Hz";
    }
}