using System.Collections.Generic;
using System.Linq;
using EngineExtensions.Core.Scheduler;
using UnityEngine;
using UnityEngine.AI;

namespace EngineExtensions.Systems.AI.Utility {
    [RequireComponent(typeof(NavMeshAgent), typeof(IVision))]
    public class Brain : UnityUpdatable {

        private List<IContextShare> _shares;
        
        public List<ActionAI> Actions;
        public Context Context;
        public float UpdateInterval = 0.25f;
        
        private float _updateCounter;

        private void Awake() {
            Context = new Context(this);
            foreach (var action in Actions) {
                action.Initialize(Context);
            }
        }

        public override void Tick(float delta) {
            if ((_updateCounter += delta) < UpdateInterval) return;
            _updateCounter -= UpdateInterval;
            UpdateContext();
            
            ActionAI bestAction = null;
            float highestUtility = 0f;
            foreach (var action in Actions) {
                var utility = action.CalculateUtility(Context);
                if (utility > highestUtility) {
                    highestUtility = utility;
                    bestAction = action;
                }
            }

            if (bestAction) {
                bestAction.Execute(Context);
            }
        }

        public void AddShare(IContextShare share) {
            _shares.Add(share);
        }

        public void RemoveShare(IContextShare share) {
            _shares.Remove(share);
        }
        
        public void RemoveAllShares() {
            _shares.Clear();
        }

        void UpdateContext() {
            foreach (var data in _shares.Select(share => share.ShareContext())) {
                foreach (var (key, value) in data) Context.SetData(key, value);
            }
        }
    }
}