using System;
using System.Collections.Generic;
using EngineExtensions.Abstractions.Utils;

namespace EngineExtensions.Input.Commands {
    public sealed class CommandPacketWriter {
        private readonly CommandRegistry _reg; public CommandPacketWriter(CommandRegistry reg){ _reg=reg; }
        public byte[] Write(int tick, int playerId, (ushort typeId, byte[] canonical)[] commands){
            var w = new ByteWriter(64);
            w.WriteByte(CommandSchema.Version);
            w.WriteVarUInt((uint)tick);
            w.WriteVarUInt((uint)playerId);
            w.WriteVarUInt((uint)commands.Length);
            for (int i=0;i<commands.Length;i++){
                var (type, canon) = commands[i];
                w.WriteVarUInt(type);
                _reg.Get(type).Write(ref w, canon);
            }
            return w.ToArray();
        }

        public byte[] Write(int tick, int playerId, IReadOnlyList<(ushort typeId, byte[] canonical)> commands) {
            var w = new ByteWriter(64);
            w.WriteByte(CommandSchema.Version);
            w.WriteVarUInt((uint)tick);
            w.WriteVarUInt((uint)playerId);
            w.WriteVarUInt((uint)commands.Count);
            for (int i=0;i<commands.Count;i++){
                var (type, canon) = commands[i];
                w.WriteVarUInt(type);
                _reg.Get(type).Write(ref w, canon);
            }
            return w.ToArray();
        }
    }
}