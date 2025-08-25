using System;

namespace EngineExtensions.Abstractions {
    public interface IScheduler {
        IDisposable Schedule(IUpdatable updatable, UpdatePhase phase, int order = 0);
        void Unschedule(IUpdatable updatable, UpdatePhase phase);
    }
    
    public interface IUpdatable {
        void Tick(float delta);
    }
}