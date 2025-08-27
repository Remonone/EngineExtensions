using EngineExtensions.Systems.AI.Utility.Considerations;
using UnityEngine;

namespace EngineExtensions.Systems.AI.Utility {
    public abstract class ActionAI : ScriptableObject {

        public string TargetTag;
        public Consideration Consideration;
        public void Initialize(Context context) { }

        public float CalculateUtility(Context context) => Consideration.Evaluate(context);

        public abstract void Execute(Context context);
    }
}