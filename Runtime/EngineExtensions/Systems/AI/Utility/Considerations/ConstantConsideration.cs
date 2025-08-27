namespace EngineExtensions.Systems.AI.Utility.Considerations {
    public class ConstantConsideration : Consideration {
        public float value;

        public override float Evaluate(Context context) => value;
    }
}