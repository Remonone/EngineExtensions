using System.Collections.Generic;
using EngineExtensions.Input.Commands;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EngineExtensions.Input.Unity {
    public sealed class UnityCommandRouter : ILocalCommandSource, IInputReader {
        private readonly InputActionAsset _asset;
        private readonly List<CommandBinding> _bindings = new();
        private ushort _heldMask;


        public UnityCommandRouter(InputActionAsset asset){ _asset = asset; }


        public void Register(CommandBinding binding){ _bindings.Add(binding); }


        public void CollectCommands(int playerIndex, in PrevState prev, List<(ushort type, byte[] canonical)> outList){
            foreach (var b in _bindings){
                bool fire = FirePolicy.HOLD_EVERY_TICK.Equals(b.Policy) || b.ShouldFire(this, prev);
                if (!fire) continue;
                var canon = b.Build(this);
                outList.Add((b.CommandType, canon));
            }
        }


        public bool GetButton(string path){ var a=_asset.FindAction(path, true); return a.ReadValue<float>() > 0.5f; }
        public float GetAxis1D(string path){ var a=_asset.FindAction(path, true); return a.ReadValue<float>(); }
        public Vector2 GetAxis2D(string path){ var a=_asset.FindAction(path, true); return a.ReadValue<Vector2>(); }
        
    }
}