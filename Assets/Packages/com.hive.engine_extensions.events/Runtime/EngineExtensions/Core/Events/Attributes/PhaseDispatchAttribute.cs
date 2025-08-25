using System;
using EngineExtensions.Abstractions;

namespace EngineExtensions.Core.Events.Attributes {

    public enum DispatchMode {
        IMMEDIATE,
        DELAYED
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class PhaseDispatchAttribute : Attribute {
        public readonly UpdatePhase Phase;
        public readonly DispatchMode Mode;
        public readonly int MaxPerTick; // 0 = unlimited
        public PhaseDispatchAttribute(UpdatePhase phase, DispatchMode mode, int maxPerTick = 0) {
            Phase = phase;
            Mode = mode;
            MaxPerTick = maxPerTick;
        }
    }
}