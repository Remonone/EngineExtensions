using System.Collections.Generic;
using EngineExtensions.Abstractions.Events;
using EngineExtensions.Abstractions.Utils;
using EngineExtensions.Core.Events.Attributes;
using EngineExtensions.Core.Events.Registry;
using EngineExtensions.Core.Events.Utils;
using NUnit.Framework;

namespace EngineExtensions.Tests {
    [WireEvent("Dummy.Event")]
    public class DummyEvent : IEvent {
        [WireField("A")] public uint A { get; set; }
        [WireField("B")] public int B { get; set; }
        [WireField("S")] public string S { get; set; }
        [WireField("Ok")] public bool Ok { get; set; }
    }

    [WireEvent("Cast.Start")]
    public class Cast : IEvent {
        [WireField("Caster")] public uint Caster { get; set; } 
        [WireField("Ability")] public uint Ability { get; set; }
    }

    [WireEvent("Hit.Done")]
    public class Hit : IEvent {
        [WireField("Target")] public uint Target { get; set; } 
        [WireField("Dmg")] public int Dmg { get; set; }
    }


    public class WiringTest {
        [Test]
        public void Roundtrip_DummyEvent() {
            var reg = new WireEventRegistry(); 
            reg.AutoRegisterFrom(typeof(DummyEvent).Assembly);
            var ev = new DummyEvent{ A=42, B=-5, S="hi", Ok=true };
            Assert.IsTrue(reg.TryGetByType(typeof(DummyEvent), out _), "AutoRegisterFrom didn't pick DummyEvent");
            var bytes = reg.EncodeObject(ev);
            var typeId = HashUtil.Fnv1a32Lower("Dummy.Event");
            var obj = (DummyEvent)reg.DecodeObject(typeId, bytes);
            Assert.AreEqual(42u, obj.A); 
            Assert.AreEqual(-5, obj.B); 
            Assert.AreEqual("hi", obj.S); 
            Assert.IsTrue(obj.Ok);
        }
        
        [Test]
        public void WriteRead_Block() {
            var reg = new WireEventRegistry(); 
            reg.AutoRegisterFrom(typeof(WiringTest).Assembly);
            var events = new List<object> {
                new Cast{ Caster=1, Ability=5 }, 
                new Hit{ Target=2, Dmg=10 }
            };
            var w = new ByteWriter(64); 
            EventBlock.Write(ref w, reg, events);
            var r = new ByteReader(w.ToArray()); 
            var list = EventBlock.Read(ref r, reg);
            Assert.AreEqual(2, list.Count);
            Assert.IsInstanceOf<Cast>(list[0]); Assert.IsInstanceOf<Hit>(list[1]);
            Assert.AreEqual(1u, ((Cast)list[0]).Caster); Assert.AreEqual(10, ((Hit)list[1]).Dmg);
        }
    }
}