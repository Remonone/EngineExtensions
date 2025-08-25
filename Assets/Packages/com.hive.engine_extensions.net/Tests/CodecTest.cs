using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Net;
using EngineExtensions.Abstractions.Utils;
using NUnit.Framework;

namespace EngineExtensions.Tests {
    public class CodecTests {
        [Test]
        public void Header_Roundtrip_Works() {
            var h = new NetMsgHeader(MsgType.SNAPSHOT, tick: 123456, seq: 42, ackMask: 0xAABBCCDD);
            var w = new ByteWriter(32);
            NetMsgHeader.Write(ref w, in h);
            var data = w.ToArray();
            var r = new ByteReader(data);
            var back = NetMsgHeader.Read(ref r);

            Assert.AreEqual(h.Type, back.Type);
            Assert.AreEqual(h.Tick, back.Tick);
            Assert.AreEqual(h.Seq, back.Seq);
            Assert.AreEqual(h.AckMask, back.AckMask);
            Assert.IsTrue(r.Eof);
        }

        [Test]
        public void VarUInt_EncodesCommonValues_Compact() {
            // values and expected length (bytes)
            (uint v, int len)[] cases = { (0u,1), (127u,1), (128u,2), (16384u,3), (0x0FFFFFFFu,4) };
            foreach (var (v, len) in cases) {
                var w = new ByteWriter(8); VarInt.WriteVarUInt(w, v); var a = w.ToArray();
                Assert.AreEqual(len, a.Length, $"value {v}");
                var r = new ByteReader(a); var back = VarInt.ReadVarUInt(ref r); Assert.AreEqual(v, back);
            }
        }
    }
}