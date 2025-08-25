using System;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Net;
using EngineExtensions.Net.Channels;
using NUnit.Framework;

namespace EngineExtensions.Tests {

    [TestFixture]
    public class SnapshotChannelTests {
        [Test]
        public void Broadcast_Then_TryGetLatest_YieldsSameTickAndPayload() {
            var tr = new LoopbackTransport();
            var clock = new FakeClock();
            var tel = new TelemetryCollector();
            var ch = new SnapshotChannel(tr, eventCode: 2, channel: 1, rel: Reliability.UNRELIABLE, 0.001, () => clock.Now, tel.Emit);

            var tick = 777;
            var payload = new byte[] { 1, 2, 3, 4, 5 };
            ch.Broadcast(tick, isKeyframe: true, payload);
            
            ch.Pump();

            Assert.IsTrue(ch.TryGetLatest(out var snap));
            Assert.AreEqual(tick, snap.Tick);
            Assert.IsTrue(snap.IsKeyframe);
            CollectionAssert.AreEqual(payload, snap.Payload.ToArray());
            Assert.GreaterOrEqual(tel.CountByName("net.recv_batch"), 1);

            (ch as IDisposable).Dispose();
        }
    }
}