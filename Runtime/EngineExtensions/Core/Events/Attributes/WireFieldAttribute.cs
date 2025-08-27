using System;

namespace EngineExtensions.Core.Events.Attributes {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited=true, AllowMultiple=false)]
    
    public sealed class WireFieldAttribute : Attribute {
        public readonly string Key;
        public uint? Tag;
        public bool Required;
        public object DefaultValue;
        public WireFieldAttribute(string key){ Key = key; }
    }
}