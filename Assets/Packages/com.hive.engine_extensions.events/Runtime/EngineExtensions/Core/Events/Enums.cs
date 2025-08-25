namespace EngineExtensions.Core.Events {
    public enum ErrorPolicy {
        SWALLOW,
        HALT,
        REPEAT
    }

    public enum Priority {
        LOWEST,
        LOW,
        MEDIUM,
        HIGH,
        HIGHEST,
    }
}