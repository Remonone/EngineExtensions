using System;
using EngineExtensions.Input.Commands;
using EngineExtensions.Input.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace EngineExtensions.Tests {
    
    public class RouterTest {
        InputActionAsset MakeAsset(){
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Gameplay");
            var jump = map.AddAction("Jump", InputActionType.Button);
            var fire = map.AddAction("Fire", InputActionType.Button);
            asset.AddActionMap(map);
            map.Enable();
            return asset;
        }
        
        [Test]
        public void Router_Fires_Registered_Commands() {
            var asset = MakeAsset();
            var reg = new CommandRegistry(); reg.Register(new JumpCodec()); reg.Register(new FireCodec());
            var adapter = new UnityInputAdapter(asset, reg);


// Bindings: Jump (edge) and Fire (hold with power=7)
            adapter.Router.Register(new CommandBinding{
                CommandType = JumpCodec.Id,
                Policy = FirePolicy.EDGE,
                ActionDependencies = new[]{ "Gameplay/Jump" },
                ShouldFire = (input, prev) => input.GetButton("Gameplay/Jump") && !prev.WasHeld(0),
                Build = _ => Array.Empty<byte>()
            });
            adapter.Router.Register(new CommandBinding{
                CommandType = FireCodec.Id,
                Policy = FirePolicy.HOLD_EVERY_TICK,
                ActionDependencies = new[]{ "Gameplay/Fire" },
                ShouldFire = (input, prev) => input.GetButton("Gameplay/Fire"),
                Build = _ => FireCodec.Canon(7)
            });


// Simulate button values by setting states
            var map = asset.FindActionMap("Gameplay", true);
            map.FindAction("Jump", true).AddBinding("<Keyboard>/space");
            map.FindAction("Fire", true).AddBinding("<Keyboard>/f");


// Manually feed device state
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
            var kb = InputSystem.AddDevice<Keyboard>();
            try {
// tick 0: press space & f
                InputSystem.QueueStateEvent(kb, new KeyboardState(Key.Space, Key.F));
                InputSystem.Update();
                var pkt0 = adapter.BuildPacketForTick(0, 0, new PrevState(0));
                var reader = new CommandPacketReader(reg);
                var decoded0 = reader.Read(pkt0);
                Assert.AreEqual(2, decoded0.Commands.Length);


// tick 1: keep holding F, release space
                InputSystem.QueueStateEvent(kb, new KeyboardState(Key.F));
                InputSystem.Update();
                var pkt1 = adapter.BuildPacketForTick(1, 0, new PrevState(1<<0)); // bit0 was held for Jump
                var decoded1 = reader.Read(pkt1);
// should contain only Shoot (hold)
                Assert.AreEqual(1, decoded1.Commands.Length);
                Assert.AreEqual(FireCodec.Id, decoded1.Commands[0].TypeId);
            } finally {
                InputSystem.RemoveDevice(kb);
            }
        }
    }
}