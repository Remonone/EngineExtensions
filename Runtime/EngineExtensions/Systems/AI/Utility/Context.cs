using System.Collections.Generic;
using EngineExtensions.Abstractions;
using EngineExtensions.Logger;
using UnityEngine.AI;

namespace EngineExtensions.Systems.AI.Utility {
    public class Context {
        private readonly Dictionary<string, object> _data = new();
        private readonly Brain _brain;
        private readonly IVision _vision;
        private NavMeshAgent _agent;

        public Brain Brain => _brain;
        public IVision Vision => _vision;
        public NavMeshAgent Agent => _agent;
        
        
        public Context(Brain brain) {
            if(brain == null)
                throw new System.ArgumentNullException(nameof(brain));
            _brain = brain;
            _vision = brain.gameObject.GetComponent<IVision>();
        }
        
        public void SetData(string key, object value) {
            if (_data.ContainsKey(key)) {
                DefaultLogger.Instance.Write(LogLevel.WARNING, $"Context already contains key {key}");
            }
            _data[key] = value;
        }
        
        public T GetData<T>(string key) {
            return (T)_data[key];
        }
    }
}