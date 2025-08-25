using System;
using System.Linq;
using EngineExtensions.Abstractions;
using EngineExtensions.Net.Channels;
using NUnit.Framework;

namespace EngineExtensions.Tests {
    public class CommandBatchingTests {
        [Test]
        public void MultipleCommands_AreBatchedAndDelivered_PerTick() {
            var clock = new FakeClock();
            var tr = new LoopbackTransport();
            var tel = new TelemetryCollector();


            var ch = new CommandChannel(tr, eventCode: 1, channel: 0, rel: Reliability.UNRELIABLE,
                sendIntervalSec: 0.05, now: clock.Get, emit: tel.Emit);


// До Pump ничего не доставлено
            ch.Send(playerId: 1, tick: 100, payload: new byte[]{ 9,9 });
            ch.Send(playerId: 1, tick: 101, payload: new byte[]{ 8,8,8 });


            Assert.IsEmpty(ch.DequeueForTick(100));
            Assert.IsEmpty(ch.DequeueForTick(101));


            ch.Pump();


            var list100 = ch.DequeueForTick(100).ToList();
            var list101 = ch.DequeueForTick(101).ToList();
            Assert.AreEqual(1, list100.Count);
            Assert.AreEqual(1, list101.Count);
            CollectionAssert.AreEqual(new byte[]{9,9}, list100[0].Payload.ToArray());
            CollectionAssert.AreEqual(new byte[]{8,8,8}, list101[0].Payload.ToArray());


            Assert.GreaterOrEqual(tel.CountByName("net.send_batch"), 1);
            Assert.GreaterOrEqual(tel.CountByName("net.recv_batch"), 1);


            (ch as IDisposable).Dispose();
        }
    }
}