using EngineExtensions.Abstractions;
using EngineExtensions.Core.Events;
using EngineExtensions.Core.Scheduler;
using UnityEngine;

namespace Sandbox.SandboxScripts {
    public class SchedulerContainer : MonoBehaviour {
        
        [SerializeField] private int _maxEventsNetFixed = 25;
        
        public float netFixedRate = 60f;
        private float _accum;
        private float _netFixedDt;
        private Scheduler _core;


        void Awake(){
            _core = new Scheduler();
            _netFixedDt = 1f / Mathf.Max(1f, netFixedRate);
        }

        private void Start() {
            _core.Schedule(new EventPump(UpdatePhase.INPUT, 0), UpdatePhase.INPUT, int.MaxValue);
            _core.Schedule(new EventPump(UpdatePhase.NET_FIXED, _maxEventsNetFixed), UpdatePhase.NET_FIXED, int.MaxValue);
            _core.Schedule(new EventPump(UpdatePhase.LATE, 0), UpdatePhase.LATE);
            _core.Schedule(new EventPump(UpdatePhase.PRESENTATION, 0), UpdatePhase.PRESENTATION, int.MinValue);
        }

        public IScheduler Scheduler => _core;
        
        void Update(){
            _core.RunPhase(UpdatePhase.EARLY, Time.deltaTime);
            _core.RunPhase(UpdatePhase.INPUT, Time.deltaTime);


            _accum += Time.deltaTime;
            while (_accum >= _netFixedDt) {
                _core.RunPhase(UpdatePhase.NET_FIXED, _netFixedDt);
                _accum -= _netFixedDt;
            }


            _core.RunPhase(UpdatePhase.LATE, Time.deltaTime);
            _core.RunPhase(UpdatePhase.PRESENTATION, Time.deltaTime);
        }
    }
}