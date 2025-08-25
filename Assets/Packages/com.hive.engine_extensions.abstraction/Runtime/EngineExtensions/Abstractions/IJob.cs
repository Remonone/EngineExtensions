using EngineExtensions.Abstractions.Telemetry;

namespace EngineExtensions.Abstractions {
    public interface IJob {
        bool ExecuteStep();
    }

    public interface IJobScheduler {
        void Enqueue(IJob job, JobPriority priority = JobPriority.MEDIUM);
        JobMetrics RunBudgeted(double msBudget);
    }
}