using EngineExtensions.Abstractions;

namespace EngineExtensions.Core.Scheduler {
    public readonly struct PhaseSample {
        public readonly UpdatePhase Phase;
        public readonly float Dt;
        public readonly double ElapsedMs;
        public readonly double? BudgetMs;
        public readonly bool OverBudget;
        public readonly int Subscribers;
        public readonly long FrameIndex;
        public PhaseSample(UpdatePhase phase, float dt, double elapsedMs,
            double? budgetMs, bool overBudget, int subscribers, long frameIndex) {
            Phase = phase; Dt = dt; ElapsedMs = elapsedMs; BudgetMs = budgetMs;
            OverBudget = overBudget; Subscribers = subscribers; FrameIndex = frameIndex;
        }
    }
}