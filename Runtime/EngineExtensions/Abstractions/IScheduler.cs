namespace EngineExtensions.Abstractions {
    public interface IScheduler {
        void Schedule(IUpdatable updatable, UpdatePhase phase);
        void Unschedule(IUpdatable updatable, UpdatePhase phase);
    }
}