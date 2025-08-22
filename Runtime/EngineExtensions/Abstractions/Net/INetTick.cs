namespace EngineExtensions.Abstractions.Net {
    public interface INetTick {
        int Tick { get; }
        float TickDelta { get; }
    }
}