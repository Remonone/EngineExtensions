using System;
using System.Collections.Generic;
using UnityEngine;

namespace EngineExtensions.Systems.AI.Utility.Considerations {
    public class CompositeConsideration : Consideration {
        public enum OperationType { AVERAGE, MULTIPLY, ADD, SUBTRACT, DIVIDE, MAX, MIN }

        public bool AllMustBeNonZero = true;

        public OperationType Operation = OperationType.MAX;
        public List<Consideration> Considerations;

        public override float Evaluate(Context context) {
            if (Considerations == null || Considerations.Count == 0) return 0f;

            float result = Considerations[0].Evaluate(context);
            if (result == 0f && AllMustBeNonZero) return 0f;

            for (int i = 1; i < Considerations.Count; i++) {
                float value = Considerations[i].Evaluate(context);
                if (value == 0f && AllMustBeNonZero) return 0f;

                switch (Operation) {
                    case OperationType.MAX:
                        result = Math.Max(result, value);
                        break;
                    case OperationType.MIN:
                        result = Math.Min(result, value);
                        break;
                    case OperationType.ADD:
                        result += value;
                        break;
                    case OperationType.SUBTRACT:
                        result -= value;
                        break;
                    case OperationType.MULTIPLY:
                        result *= value;
                        break;
                    case OperationType.DIVIDE:
                        result = value != 0 ? result / value : result;
                        break;
                    case OperationType.AVERAGE:
                        result = (result + value) / 2f;
                        break;
                }
            }

            return Mathf.Clamp01(result);
        }
    }
}