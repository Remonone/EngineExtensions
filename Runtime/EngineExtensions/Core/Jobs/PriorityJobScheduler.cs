using System;
using System.Collections.Generic;
using System.Diagnostics;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Telemetry;

namespace EngineExtensions.Core.Jobs {
    public class PriorityJobScheduler : IJobScheduler {
        readonly Queue<IJob>[] _q = { new(), new(), new() };
        JobMetrics _last;
        public JobMetrics LastMetrics => _last;
        public event Action<JobMetrics> OnMetrics;
        
        public void Enqueue(IJob job, JobPriority p = JobPriority.MEDIUM) => _q[(int)p].Enqueue(job);
        
        public JobMetrics RunBudgeted(double msBudget) {
            var sw = Stopwatch.StartNew();
            int steps = 0, completed = 0;
            bool over = false;
            int safety = 100000;

            while (sw.Elapsed.TotalMilliseconds < msBudget && safety-- > 0) {
                var job = Dequeue();
                if (job == null) break;
                bool done = job.ExecuteStep();
                steps++;
                if (!done) Requeue(job);
                else completed++;
            }
            sw.Stop();
            if (sw.Elapsed.TotalMilliseconds > msBudget) over = true;

            _last = new JobMetrics(
                steps, completed,
                _q[0].Count, _q[1].Count, _q[2].Count, over
            );
            OnMetrics?.Invoke(_last);
            return _last;
        }
        
        IJob Dequeue() {
            for (int i=0;i<_q.Length;i++) if (_q[i].Count>0) return _q[i].Dequeue();
            return null;
        }
        void Requeue(IJob job) { _q[(int)JobPriority.MEDIUM].Enqueue(job); }
        
    }
}