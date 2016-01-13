using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Diagnostics;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace AsyncProfilingDemo.Web.Controllers
{

    public class TestController : ApiController
    {
        private readonly IIoService _ioService;
        private readonly ICpuService _cpuService;
        private readonly IDemoInterception _intercepted;
        
        public TestController(IIoService io, ICpuService cpu, IDemoInterception intercepted)
        {
            _ioService = io;
            _cpuService = cpu;
            _intercepted = intercepted;
        }


        [HttpGet]
        [ProfileAction]
        public async Task<IHttpActionResult> GoAsync()
        {
            var timer = new Stopwatch();
            timer.Start();

            var io1 = await _ioService.OperationOneAsync(); //300 ms
            var io2 = await _ioService.OperationTwoAsync(); //3000 ms
            var cpu = _cpuService.Compute(); //125 ms

            timer.Stop();

            var result = io1 + io2 + cpu;
            var message = string.Format("Action took {0} ms and found result {1}", timer.ElapsedMilliseconds, result);

            return Ok(message);
        }

        [HttpGet]
        public async Task<IHttpActionResult> ProfiledGoAsync()
        {
            var timer = new Stopwatch();
            timer.Start();

            MiniProfiler.Start();

            int io1, io2, cpu;

            using(MiniProfiler.Current.Step("OperationOne"))
            {
                io1 = await _ioService.OperationOneAsync();
            }
            using(MiniProfiler.Current.Step("OperationTwo"))
            {
                io2 = await _ioService.OperationTwoAsync();
            }
            using(MiniProfiler.Current.Step("Compute"))
            {
                cpu = _cpuService.Compute();
            }

            var result = io1 + io2 + cpu;
            timer.Stop();

            var message = string.Format("Action took {0} ms and found result {1}", timer.ElapsedMilliseconds, result);

            MiniProfiler.Stop();
            Debug.WriteLine(MiniProfiler.Current.RenderPlainText());
            return Ok(message);
        }

        [HttpGet]
        [ProfileAction]
        public async Task<IHttpActionResult> ThrowExceptionAsync()
        {
            await _ioService.ThrowExceptionAsync();
            return Ok();
        }

        [HttpGet]
        public IHttpActionResult DemoInterceptor()
        {
            var result = _intercepted.GetResult();
            return Ok(result);
        }

        [HttpGet]
        public IHttpActionResult TimingLogs()
        {
            var logs = TimingLog.RollingTimingLog.ToList();
            return Ok(logs);
        }

        [HttpGet]
        public IHttpActionResult Profile(string actionName)
        {
            var logs = TimingLog.RollingTimingLog.ToArray().Where(x => x.ActionName == actionName);
            var total = logs.Sum(x => x.TotalMilliseonds);
            
            var response = new JObject();

            foreach(var log in logs)
            {
                foreach(var method in log.TimingDetails)
                {
                    if(response.Property(method.StepName) == null)
                    {
                        response.Add(method.StepName, method.Milliseconds / total);
                    }
                    else
                    {
                        response[method.StepName] = (decimal)response[method.StepName] + method.Milliseconds / total;
                    }
                }
            }

            foreach(var property in response)
            {
                response[property.Key] = Math.Round((decimal)property.Value * 100) + " %";
            }

            return Ok(response);
        }
    }
}