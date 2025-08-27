using EngineExtensions.Abstractions;
using UnityEngine;

namespace EngineExtensions.Core.Scheduler {
    public abstract class UnityUpdatable : MonoBehaviour, IUpdatable {
        [SerializeField] private UpdatePhase _phase;
        [SerializeField] private int _order = 0;

        protected virtual void OnEnable() {
            if(SchedulerLocator.Has) SchedulerLocator.Instance.Schedule(this, _phase, _order);
        }
        
        protected virtual void OnDisable() {
            if(SchedulerLocator.Has) SchedulerLocator.Instance.Unschedule(this, _phase);
        }

        public abstract void Tick(float delta);
    }
}