namespace EngineExtensions.Input.Commands {
    
    public enum FirePolicy { EDGE, HOLD_EVERY_TICK }
    public delegate bool ShouldFire(IInputReader input, PrevState prev);
    public delegate byte[] BuildCanonical(IInputReader input);


    public readonly struct PrevState { public readonly ushort HeldMask; public PrevState(ushort held){ HeldMask=held; } public bool WasHeld(int bit)=> (HeldMask & (1u<<bit))!=0; }
    
    public sealed class CommandBinding {
        public ushort CommandType;
        public FirePolicy Policy;
        public string[] ActionDependencies;
        public ShouldFire ShouldFire;
        public BuildCanonical Build;
    }
}