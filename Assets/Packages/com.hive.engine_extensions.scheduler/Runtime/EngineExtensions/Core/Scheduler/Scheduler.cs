using System;
using System.Collections.Generic;
using System.Diagnostics;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Telemetry;

namespace EngineExtensions.Core.Scheduler {
    internal sealed class Entry {
        public readonly IUpdatable Updatable;
        public readonly int Order;
        public Entry(IUpdatable updatable, int order) { Updatable = updatable; Order = order; }
    }
    
    public class Scheduler : IScheduler, ITelemetrySource {
        private readonly Dictionary<UpdatePhase, List<Entry>> _subscriptions = new();
        private readonly Dictionary<UpdatePhase, List<PhaseSample>> _history = new();
        private readonly Stopwatch _sw = new();
        private readonly int _historySize;
        private long _frameIndex;
        private ulong _spanSeq;
        private readonly Dictionary<UpdatePhase, double?> _budgetMs = new();

        public Scheduler(int historySize = 255) {
            _historySize = Math.Max(1, historySize);
            foreach (UpdatePhase phase in Enum.GetValues(typeof(UpdatePhase))) {
                _subscriptions[phase] = new List<Entry>();
                _history[phase] = new List<PhaseSample>();
                _budgetMs[phase] = null;
            }
        }
        
        public IDisposable Schedule(IUpdatable updatable, UpdatePhase phase, int order = 0) {
            var list = _subscriptions[phase];
            int i = list.Count;
            while (i > 0 && list[i - 1].Order > order) i--;
            list.Insert(i, new Entry(updatable, order));
            return new Subscription(this, updatable, phase);
        }

        public void Unschedule(IUpdatable updatable, UpdatePhase phase) {
            var list = _subscriptions[phase];
            for (int i = 0; i < list.Count; i++) {
                if (ReferenceEquals(list[i].Updatable, updatable)) {
                    list.RemoveAt(i);
                    break;
                }
            }
        }

        public void SetPhaseBudget(UpdatePhase phase, double? budgetMs) {
            _budgetMs[phase] = budgetMs;
        }

        public void RunPhase(UpdatePhase phase, float dt) {
            var list = _subscriptions[phase];
            var budget = _budgetMs[phase];

            var spanId = NextSpanId();
            Emit(new TelemetryMarker(
                TelemetryKind.SPAN_START, "scheduler.phase", TelemetryMarker.NowMicros(),
                spanId, null,
                tags: new[] {
                    new TelemetryTag("phase", phase.ToString())
                }, fields: new[] {
                    new TelemetryField("dt_s", dt, "s"),
                    new TelemetryField("subscribes", list.Count, ""),
                    new TelemetryField("budget_ms", budget ?? 0, "ms")
                })
            );
            _sw.Restart();
            for (int i = 0; i < list.Count; i++) {
                list[i].Updatable.Tick(dt);
            }
            _sw.Stop();
            
            double elapsed = _sw.Elapsed.TotalMilliseconds;
            bool over = budget.HasValue && elapsed > budget.Value;

            var sample = new PhaseSample(phase, dt, elapsed, budget, over, list.Count, _frameIndex++);
            var history = _history[phase];
            if(history.Count >= _historySize) history.RemoveAt(0);
            history.Add(sample);
            OnPhaseSampled?.Invoke(sample);
            var tags = over 
                ? new[] { new TelemetryTag("phase", phase.ToString()), new TelemetryTag("over_budget", "true") }
                : new[] { new TelemetryTag("phase", phase.ToString()) };
            Emit(new TelemetryMarker(
                TelemetryKind.SPAN_END, "scheduler.phase", TelemetryMarker.NowMicros(),
                spanId, null,
                tags: tags,
                fields: new[] {
                    new TelemetryField("elapsed_ms", elapsed, "ms"),
                    new TelemetryField("dt_s", dt, "s"),
                    new TelemetryField("subscribes", list.Count, ""),
                    new TelemetryField("frame_index", _frameIndex - 1, "count"),
                    new TelemetryField("budget_ms", budget ?? 0, "ms")
                }
            ));
        }
        
        public PhaseSample Last(UpdatePhase phase) {
            var h = _history[phase];
            return h.Count > 0 ? h[^1] : default;
        }
        
        private ulong NextSpanId() => ++_spanSeq;
        private void Emit(in TelemetryMarker m) => OnTelemetry?.Invoke(m);

        public event Action<TelemetryMarker> OnTelemetry;
        public event Action<PhaseSample> OnPhaseSampled;
        
        private readonly struct Subscription : IDisposable {
            private readonly Scheduler _scheduler; 
            private readonly IUpdatable _updatable; 
            private readonly UpdatePhase _phase;
            public Subscription(Scheduler scheduler, IUpdatable updatable, UpdatePhase phase){ _scheduler=scheduler; _updatable=updatable; _phase=phase; }
            public void Dispose() => _scheduler.Unschedule(_updatable, _phase);
        }
    }
}