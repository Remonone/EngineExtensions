using System.Collections.Generic;
using System.Runtime.InteropServices;
using EngineExtensions.Input.Commands;
using UnityEngine.InputSystem;

namespace EngineExtensions.Input.Unity {
    public sealed class UnityInputAdapter {
        public CommandRegistry Registry { get; }
        public UnityCommandRouter Router { get; }
        public CommandPacketWriter PacketWriter { get; }


        public UnityInputAdapter(InputActionAsset asset, CommandRegistry registry){
            Registry = registry; Router = new UnityCommandRouter(asset); PacketWriter = new CommandPacketWriter(registry);
        }


        /// Call inside your Scheduler's Input phase (after InputSystem.Update()).
        public byte[] BuildPacketForTick(int tick, int playerId, in PrevState prev){
            var list = new List<(ushort, byte[])>(8);
            Router.CollectCommands(playerId, prev, list);
            return PacketWriter.Write(tick, playerId, list);
        }
    }
}