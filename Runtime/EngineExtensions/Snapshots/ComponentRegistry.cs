using System;
using System.Collections.Generic;
using EngineExtensions.Abstractions.Snapshots;

namespace EngineExtensions.Snapshots {
    public sealed class ComponentRegistry {
        private readonly Dictionary<ushort, IComponentCodec> _codecs = new();
        public void Register(IComponentCodec codec){ if(codec==null) throw new ArgumentNullException(nameof(codec)); _codecs[codec.TypeId]=codec; }
        public IComponentCodec Get(ushort typeId){ if(!_codecs.TryGetValue(typeId, out var c)) throw new InvalidOperationException($"No codec for type {typeId}"); return c; }
    }
}