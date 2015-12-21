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
            
            var actionTiming = new OverallTiming
            {
                ActionName = actionName,
                TotalMilliseonds = totalTime,
                TimingDetails = new List<TimingDetail>()
            };

            var methodInvocations = new Dictionary<string, int>();
            foreach(var timing in MiniProfiler.Current.GetTimingHierarchy())
            {
                if(timing.IsRoot)
                {
                    continue;
                }
                
                var methodName = timing.Name;
                int invocations;
                if (!methodInvocations.TryGetValue(methodName, out invocations))
                {
                    methodInvocations[methodName] = 1;
                }
                
                var methodId = timing.Name + "_" + invocations;
                
                actionTiming.TimingDetails.Add(new TimingDetail { MethodId = methodId, Milliseconds = timing.DurationMilliseconds.GetValueOrDefault() });
            }
            TimingLog.RollingTimingLog.Enqueue(actionTiming);
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
        public string MethodId {get; set;}
        public decimal Milliseconds {get; set;}
    }
}