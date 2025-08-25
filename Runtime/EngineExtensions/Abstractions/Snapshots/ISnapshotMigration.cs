namespace EngineExtensions.Abstractions.Snapshots {
    public interface ISnapshotMigration {
        byte From { get; } 
        byte To { get; } 
        byte[] Migrate(byte[] oldBytes);
    }
}