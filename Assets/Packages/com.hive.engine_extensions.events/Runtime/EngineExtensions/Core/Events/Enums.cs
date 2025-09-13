namespace EngineExtensions.Core.Events {
    public enum ErrorPolicy {
        SWALLOW,
        HALT,
        REPEAT
    }

    public enum Priority {
        LOWEST = 0,
        LOW = 1,
        MEDIUM = 2,
        HIGH = 3,
        HIGHEST = 4,
    }
}