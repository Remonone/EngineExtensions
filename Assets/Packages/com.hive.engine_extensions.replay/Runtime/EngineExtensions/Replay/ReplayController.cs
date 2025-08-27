using System;
using System.Collections.Generic;

namespace EngineExtensions.Replay {
    public sealed class ReplayController {
        public float Speed { get; set; } = 1f;
        public bool Paused { get; set; }
        public int CurrentTick { get; private set; }

        private readonly FileReplaySource _src;

        public ReplayController(FileReplaySource src){ _src = src; }

        public void Seek(int tick){ CurrentTick = tick; }

        public void StepOneTick(Action<List<(int playerId, byte[] packet)>> applyCommands, Action<byte[]> applySnapshot){
            if (_src.TryGetSnapshot(CurrentTick, out var snap)) { applySnapshot?.Invoke(snap); }
            else { var cmds = new List<(int, byte[])>(); if (_src.TryGetCommands(CurrentTick, cmds)) applyCommands?.Invoke(cmds); }
            CurrentTick++;
        }
    }
}