using UnityEngine;
using EngineExtensions.Extras.Extensions;

namespace EngineExtensions.Systems.AI.Utility.Considerations {
    public class InRangeConsideration : Consideration {
        public float MaxDistance = 10f;
        public float MaxAngle = 360f;
        public string TargetTag = "Target";
        public AnimationCurve Curve;

        public override float Evaluate(Context context) {
            
            if (!context.Vision.ObserveTags.Contains(TargetTag)) {
                context.Vision.ObserveTags.Add(TargetTag);
            }
            Transform closestTarget = GetClosestTarget(context);
            if (closestTarget == null) return 0f;
            
            Transform agentTransform = context.Agent.transform;
            
            bool isInRange = agentTransform.InRangeOf(closestTarget, MaxDistance, MaxAngle);
            if (!isInRange) return 0f;
            
            Vector3 directionToTarget = closestTarget.position - agentTransform.position;
            float distanceToTarget = directionToTarget.With(y:0).magnitude;
            
            float normalizedDistance = Mathf.Clamp01(distanceToTarget / MaxDistance);
            
            float utility = Curve.Evaluate(normalizedDistance);
            return Mathf.Clamp01(utility);
        }

        private Transform GetClosestTarget(Context context) {
            var list = context.Vision.ObjectsInVision;
            if (list == null || list.Count < 1) return null;
            var aiTransform = context.Brain.gameObject.transform;
            Transform closestTarget = null;
            var closestDistance = float.MaxValue;
            foreach (var target in list) {
                var distance = Vector3.Distance(target.position, aiTransform.position);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }

            return closestTarget;
        }

        void Reset() {
            Curve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0f)
            );
        }
    }
}