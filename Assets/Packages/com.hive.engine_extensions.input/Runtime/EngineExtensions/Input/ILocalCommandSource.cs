using System.Collections.Generic;
using EngineExtensions.Input.Commands;

namespace EngineExtensions.Input {
    public interface ILocalCommandSource {
        void CollectCommands(int playerIndex, in PrevState prevState, List<(ushort type, byte[] canonical)> outList);
    }
}