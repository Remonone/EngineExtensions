using System;
using System.Collections.Generic;
using EngineExtensions.Abstractions;

namespace EngineExtensions.Core.Events.Policies.Dispatcher {

    public interface IDefferedEnvelope {
        UpdatePhase Phase { get; }
        void DispatchNow();
    }
    
    public static class EventRouter {
        private static readonly Queue<IDefferedEnvelope>[] _queues;

        static EventRouter() {
            var phases = (UpdatePhase[])Enum.GetValues(typeof(UpdatePhase));
            _queues = new Queue<IDefferedEnvelope>[phases.Length];
            for (int i = 0; i < phases.Length; i++) {
                _queues[i] = new Queue<IDefferedEnvelope>();
            }
        }
        
        public static void Enqueue(IDefferedEnvelope envelope) {
            _queues[(int)envelope.Phase].Enqueue(envelope);
        }

        public static int Pump(UpdatePhase phase, int maxPerTick = 0) {
            var q = _queues[(int)phase];
            int processed = 0;
            int limit = maxPerTick > 0 ? Math.Min(maxPerTick, q.Count) : q.Count;
            while (processed < limit && q.Count > 0) {
                var envelope = q.Dequeue();
                envelope.DispatchNow();
                processed++;
            }
            return processed;
        }

        public static int Pending(UpdatePhase phase) { return _queues[(int)phase].Count; }
    }
}