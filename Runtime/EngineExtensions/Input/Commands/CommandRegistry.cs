using System;
using System.Collections.Generic;

namespace EngineExtensions.Input.Commands {
    public sealed class CommandRegistry {
        private readonly Dictionary<ushort, ICommandCodec> _codecs = new();

        public void Register(ICommandCodec codec) {
            if (codec == null)
                throw new ArgumentNullException(nameof(codec));
            _codecs[codec.TypeId] = codec;
        }

        public ICommandCodec Get(ushort typeId) {
            if(!_codecs.TryGetValue(typeId, out var c))
                throw new InvalidOperationException($"No codec for command {typeId}");
            return c;
        }
    }
}