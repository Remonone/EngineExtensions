namespace EngineExtensions.Abstractions {
    public interface ISaveable {
        object Snapshot();
        void Restore(object data);
    }
    
    public interface ISerializer {
        byte[] Write<T>(T obj);
        T Read<T>(byte[] bytes, int version);
    }
}