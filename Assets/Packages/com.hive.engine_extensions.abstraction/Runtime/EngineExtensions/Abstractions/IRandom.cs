namespace EngineExtensions.Abstractions {
    public interface IRandom {
        uint Next();
        int NextRange(int min,int max);
        float NextFloat(); 
        IRandom Fork(uint? seed=null);
    }
}