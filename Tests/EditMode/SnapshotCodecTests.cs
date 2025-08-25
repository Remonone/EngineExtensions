using System.Collections.Generic;
using System.Linq;
using EngineExtensions.Abstractions.Snapshots;
using EngineExtensions.Snapshots;
using NUnit.Framework;

namespace EngineExtensions.Tests {
    public class SnapshotCodecTests {
        ComponentRegistry MakeReg(){ var r=new ComponentRegistry(); r.Register(new DummyCodecs()); return r; }

        static EntityState MakeEntity(ulong id, ushort arch, int x, int y){
            var comps = new List<ComponentState>{ new ComponentState(DummyCodecs.Type, DummyCodecs.CanonicalState(x,y)) };
            return new EntityState(id, arch, comps);
        }

        [Test]
        public void Keyframe_RoundtripIdentity_Generic(){
            var reg = MakeReg();
            var frame = new SnapshotFrame(100, true, new[]{ MakeEntity(1,1, 10,20), MakeEntity(2,1, -5,7) });
            var w = new SnapshotWriter(reg); 
            var bytes = w.WriteFrame(frame);
            var r = new SnapshotReader(reg); 
            var back = r.Read(bytes);
            Assert.AreEqual(frame.Tick, back.Tick); 
            Assert.IsTrue(back.Keyframe);
            Assert.AreEqual(frame.Entities.Length, back.Entities.Length);
            var e1 = back.Entities.Single(e=>e.EntityId==1);
            var ps = DummyCodecs.Parse(e1.Components.Single().Bytes); 
            Assert.AreEqual((10,20), ps);
        }

        [Test]
        public void Delta_OneOfTwoComponents_IsSmallerThanKey(){
            var reg = new ComponentRegistry();
            reg.Register(new DummyCodecs());
            reg.Register(new DummyHealthCodec());

            var writer = new SnapshotWriter(reg);
            var reader = new SnapshotReader(reg);

            // Keyframe: entity has Position(0,0) + Health(150)
            var compsKey = new List<ComponentState>{
                new ComponentState(DummyCodecs.Type, DummyCodecs.CanonicalState(0,0)),
                new ComponentState(DummyHealthCodec.Type, DummyHealthCodec.Canonical(150))
            };
            var keyFrame = new SnapshotFrame(300, true, new[]{ new EntityState(1,1, compsKey) });
            var keyBytes = writer.WriteFrame(keyFrame);
            reader.Read(keyBytes);

            // Delta: change only Position.X to 5; HP unchanged
            var compsDelta = new List<ComponentState>{
                new ComponentState(DummyCodecs.Type, DummyCodecs.CanonicalState(5,0)),
                new ComponentState(DummyHealthCodec.Type, DummyHealthCodec.Canonical(150))
            };
            var deltaFrame = new SnapshotFrame(301, false, new[]{ new EntityState(1,1, compsDelta) });
            var deltaBytes = writer.WriteFrame(deltaFrame);
            var back = reader.Read(deltaBytes);

            var e = back.Entities.Single();
            var pos = DummyCodecs.Parse(e.Components.Single(c=>c.TypeId==DummyCodecs.Type).Bytes);
            Assert.AreEqual((5,0), pos);
            Assert.Less(deltaBytes.Length, keyBytes.Length);
        }

        [Test]
        public void Delta_RemoveComponent_RemovedFromState(){
            var reg = MakeReg(); 
            var w = new SnapshotWriter(reg); 
            var r = new SnapshotReader(reg);
            var key = new SnapshotFrame(10, true, new[]{ MakeEntity(1,1, 1,1) });
            r.Read(w.WriteFrame(key));
            // entity without the component
            var cur = new EntityState(1,1, new List<ComponentState>());
            var delta = new SnapshotFrame(11, false, new[]{ cur });
            r.Read(w.WriteFrame(delta));
            var back = r.Read(w.WriteFrame(new SnapshotFrame(12, false, new[]{ cur })));
            var e = back.Entities.Single();
            Assert.AreEqual(0, e.Components.Count);
        }

        [Test]
        public void SchemaMismatch_Throws(){
            var reg = MakeReg(); 
            var w = new SnapshotWriter(reg); 
            var r = new SnapshotReader(reg);
            var bytes = w.WriteFrame(new SnapshotFrame(1,true,new[]{ MakeEntity(1,1,0,0) }));
            bytes[0] = (byte)(SnapshotSchema.Version + 1);
            Assert.Throws<System.InvalidOperationException>(() => r.Read(bytes));
        }
    }
}