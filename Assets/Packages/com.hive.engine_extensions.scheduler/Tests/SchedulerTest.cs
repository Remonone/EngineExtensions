using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Telemetry;
using EngineExtensions.Core.Scheduler;
using NUnit.Framework;

namespace EngineExtensions.Tests {
    
    sealed class Probe : IUpdatable
    {
        readonly string _name; readonly List<string> _trace;
        public int Ticks { get; private set; }
        public Probe(string name, List<string> trace) { _name = name; _trace = trace; }
        public void Tick(float dt) { Ticks++; _trace.Add(_name); }
    }
    
    [TestFixture]
    public class SchedulerTest {
        [Test]
        public void PhaseOrderAndSampleAreCorrect() {
            var trace = new List<string>();
            var sch = new Scheduler(historySize: 8);

            var a = new Probe("A", trace);
            var b = new Probe("B", trace);

            sch.Schedule(b, UpdatePhase.INPUT,  order: 10);
            sch.Schedule(a, UpdatePhase.INPUT,  order: -10);

            PhaseSample? lastSample = null;
            (sch).OnPhaseSampled += s => { if (s.Phase == UpdatePhase.INPUT) lastSample = s; };

            sch.RunPhase(UpdatePhase.INPUT, dt: 0.016f);

            CollectionAssert.AreEqual(new[] { "A", "B" }, trace);

            Assert.IsTrue(lastSample.HasValue);
            Assert.AreEqual(UpdatePhase.INPUT, lastSample.Value.Phase);
            Assert.AreEqual(2, lastSample.Value.Subscribers);
            Assert.Greater(lastSample.Value.ElapsedMs, 0.0);
            Assert.IsFalse(lastSample.Value.OverBudget);
        }
        
        [Test]
        public void BudgetOverrunSetsFlagAndEmitsSpanMarkers() {
            var sch = new Scheduler(historySize: 4);
            sch.SetPhaseBudget(UpdatePhase.NET_FIXED, budgetMs: 0.1);

            var markers = new List<TelemetryMarker>();
            ((ITelemetrySource)sch).OnTelemetry += m => markers.Add(m);

            sch.Schedule(new Busy(5), UpdatePhase.NET_FIXED, order: 0);

            sch.RunPhase(UpdatePhase.NET_FIXED, dt: 1f / 60f);

            var sample = sch.Last(UpdatePhase.NET_FIXED);
            Assert.IsTrue(sample.OverBudget, "Awaited OverBudget=true on tiny-budget and Busy job");

            Assert.GreaterOrEqual(markers.Count, 2);
            var start = markers.FirstOrDefault(m => m.Kind == TelemetryKind.SPAN_START && m.Name == "scheduler.phase");
            var end   = markers.LastOrDefault (m => m.Kind == TelemetryKind.SPAN_END   && m.Name == "scheduler.phase");
            Assert.AreNotEqual(default(TelemetryMarker), start);
            Assert.AreNotEqual(default(TelemetryMarker), end);
            Assert.AreEqual(start.SpanId, end.SpanId);

            Assert.AreEqual("NET_FIXED", start.Tags.First(t => t.Key == "phase").Value);
            Assert.AreEqual("NET_FIXED", end  .Tags.First(t => t.Key == "phase").Value);

            var overTag = end.Tags.FirstOrDefault(t => t.Key == "over_budget");
            Assert.AreEqual("true", overTag.Value);

            var elapsedField = end.Fields.FirstOrDefault(f => f.Key == "elapsed_ms");
            Assert.Greater(elapsedField.Value, 0.0);
        }
        
        sealed class Busy : IUpdatable
        {
            readonly int _ms;
            public Busy(int ms) { _ms = ms; }
            public void Tick(float dt)
            {
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < _ms) { /* busy wait */ }
            }
        }
    }
}