using System;
using System.Reflection;
using EngineExtensions.Abstractions;
using EngineExtensions.Core.Events.Attributes;

namespace EngineExtensions.Core.Events.Policies.Dispatcher {
    public readonly struct DispatchPolicy {
        public readonly bool Delayed;
        public readonly UpdatePhase Phase;
        public readonly int MaxPerTick;
        public DispatchPolicy(bool delayed, UpdatePhase phase, int maxPerTick)
        { Delayed = delayed; Phase = phase; MaxPerTick = maxPerTick; }

        public static DispatchPolicy From(Type eventType) {
            var a = eventType.GetCustomAttribute<PhaseDispatchAttribute>();
            return a is null
                ? new DispatchPolicy(false, default, 0)
                : new DispatchPolicy(a.Mode == DispatchMode.DELAYED, a.Phase, a.MaxPerTick);
        }
    }
}