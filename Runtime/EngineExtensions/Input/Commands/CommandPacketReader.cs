using System;
using System.Collections.Generic;
using EngineExtensions.Abstractions.Utils;

namespace EngineExtensions.Input.Commands {
    public readonly struct DecodedCommand { public readonly ushort TypeId; public readonly byte[] Canonical; public DecodedCommand(ushort t, byte[] c){ TypeId=t; Canonical=c; } }
    public readonly struct CommandPacket { public readonly int Tick, PlayerId; public readonly DecodedCommand[] Commands; public CommandPacket(int t,int p,DecodedCommand[] c){ Tick=t; PlayerId=p; Commands=c ?? Array.Empty<DecodedCommand>(); } }


    public sealed class CommandPacketReader {
        private readonly CommandRegistry _reg; public CommandPacketReader(CommandRegistry r){ _reg=r; }
        public CommandPacket Read(byte[] data){
            var r = new ByteReader(data);
            var ver = r.ReadByte(); 
            if (ver != CommandSchema.Version) 
                throw new InvalidOperationException($"Unsupported command schema {ver}");
            int tick = (int)r.ReadVarUInt(); 
            int player = (int)r.ReadVarUInt(); 
            int count=(int)r.ReadVarUInt();
            var list = new List<DecodedCommand>(count);
            for (int i=0;i<count;i++){
                ushort type = (ushort)r.ReadVarUInt(); 
                var codec=_reg.Get(type); 
                var canon = codec.Read(ref r); 
                list.Add(new DecodedCommand(type, canon));
            }
            return new CommandPacket(tick, player, list.ToArray());
        }
    }
}