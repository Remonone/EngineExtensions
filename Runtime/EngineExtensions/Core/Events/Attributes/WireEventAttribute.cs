using System;

namespace EngineExtensions.Core.Events.Attributes {
    [AttributeUsage(AttributeTargets.Class, Inherited=false)]
    public sealed class WireEventAttribute : Attribute {
        public readonly string Name;
        public readonly uint? TypeId;
        public bool AutoFieldTags = true;
        public WireEventAttribute(ushort typeId){ TypeId = typeId; }
        public WireEventAttribute(string name){ Name = name; }
    }
}