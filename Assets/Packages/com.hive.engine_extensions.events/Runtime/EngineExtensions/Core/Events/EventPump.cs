using EngineExtensions.Abstractions;
using EngineExtensions.Core.Events.Policies.Dispatcher;

namespace EngineExtensions.Core.Events {
    public class EventPump : IUpdatable {
        
        private readonly UpdatePhase _phase;
        private readonly int _maxPerTick;

        public EventPump(UpdatePhase phase, int maxPerTick) {
            _phase = phase;
            _maxPerTick = maxPerTick;
        }
        public void Tick(float delta) {
            var n = EventRouter.Pump(_phase, _maxPerTick);
        }
    }
}