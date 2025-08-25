namespace EngineExtensions.Core.Jobs {
    public sealed class Decimator {
        private readonly int _n; private int _c;
        public Decimator(int everyNTicks) { _n = System.Math.Max(1, everyNTicks); }
        public bool ShouldRun() => (++_c % _n) == 0;
    }
}