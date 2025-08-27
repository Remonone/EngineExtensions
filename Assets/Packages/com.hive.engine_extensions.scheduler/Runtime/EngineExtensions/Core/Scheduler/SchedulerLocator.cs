using EngineExtensions.Abstractions;

namespace EngineExtensions.Core.Scheduler {
    public static class SchedulerLocator {
        public static IScheduler Instance { get; set; }
        public static bool Has => Instance != null;
    }
}