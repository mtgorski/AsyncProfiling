using Newtonsoft.Json.Linq;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Caching;
using System.Collections.Concurrent;

namespace AsyncProfilingDemo.Web
{
    public static class TimingLog
    {
        public static ConcurrentQueue<OverallTiming> RollingTimingLog = new ConcurrentQueue<OverallTiming>();
        public static readonly int MaxLogs = 1000;
    }

    public class ProfilingStorageHandler : DelegatingHandler
    {
        private readonly object _lock = new object();

        protected async override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            MiniProfiler.Stop();
            var action = request.GetActionDescriptor();
            if(action == null)
            {
                return response;
            }
            var actionName = action.ActionName;
            var totalTime = MiniProfiler.Current.DurationMilliseconds;
            
            var requestTiming = new OverallTiming
            {
                ActionName = actionName,
                TotalMilliseonds = totalTime,
                TimingDetails = new List<TimingDetail>()
            };

            var stepInvocations = new Dictionary<string, int>();
            foreach(var timing in MiniProfiler.Current.GetTimingHierarchy())
            {
                if(timing.IsRoot)
                {
                    continue;
                }

                var stepName = timing.Name;

                int invocations;
                stepInvocations.TryGetValue(stepName, out invocations);
                invocations++;
                
                stepInvocations[stepName] = invocations;

                stepName = stepName + "_" + invocations;
               
                requestTiming.TimingDetails.Add(new TimingDetail { StepName = stepName, Milliseconds = timing.DurationMilliseconds.GetValueOrDefault() });
            }

            var actionTiming = requestTiming.TimingDetails.SingleOrDefault(t => t.StepName == request.GetActionDescriptor().ControllerDescriptor.ControllerName + "_" + actionName + "_" + 1);
            if (actionTiming != null)
            {
                var pipelineTime = new TimingDetail { StepName = "Non-action time", Milliseconds = requestTiming.TotalMilliseonds - actionTiming.Milliseconds };
                requestTiming.TimingDetails.Add(pipelineTime);
            }
            TimingLog.RollingTimingLog.Enqueue(requestTiming);
            lock(_lock)
            {
                if(TimingLog.RollingTimingLog.Count > TimingLog.MaxLogs)
                {
                    OverallTiming removed;
                    TimingLog.RollingTimingLog.TryDequeue(out removed);
                }
            }

            return response;
        }

    }

    public class OverallTiming
    {
        public string ActionName {get; set;}
        public decimal TotalMilliseonds {get; set;}
        public List<TimingDetail> TimingDetails {get; set;}
    }

    public class TimingDetail
    {
        public string StepName {get; set;}
        public decimal Milliseconds {get; set;}
    }
}