using System;
using EngineExtensions.Net;
using EngineExtensions.Net.Channels;
using NUnit.Framework;
using UnityEngine;

namespace EngineExtensions.Tests {
    public class ControlChannelTests {
        [Test]
        public void Send_WithAck_LoopsBack_ControlAndAck() {
            var clock = new FakeClock();
            var tr = new LoopbackTransport();
            var tel = new TelemetryCollector();


            var cfg = ScriptableObject.CreateInstance<NetConfig>();
            cfg.CommandCode = 1; cfg.CommandsChannel = 0; // control = +10


            var ch = new ControlChannel(tr, cfg, now: clock.Get, emit: tel.Emit);


            bool gotCtrl = false; bool gotAck = false; uint ackSeq = 0;
            byte[] payload = { 0xCA, 0xFE };
            ch.OnControl += (sender, data) => { gotCtrl = System.Linq.Enumerable.SequenceEqual(payload, data.ToArray()); };
            ch.OnAck += a => { gotAck = true; ackSeq = a.Seq; };


            var seq = ch.Send(payload, requestAck: true);


            Assert.IsTrue(gotCtrl, "Control payload must be received back via loopback.");
            Assert.IsTrue(gotAck, "ACK must be received for requestAck=true.");
            Assert.AreEqual(seq, ackSeq);


            (ch as IDisposable).Dispose();
            UnityEngine.Object.DestroyImmediate(cfg);
        }
    }
}