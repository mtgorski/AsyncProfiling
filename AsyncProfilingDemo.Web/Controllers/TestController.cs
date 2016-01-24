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
            var io1 = await _ioService.OperationOneAsync(); //300 ms
            var io2 = await _ioService.OperationTwoAsync(); //3000 ms
            var cpu = _cpuService.Compute(); //125 ms

            return Ok(io1 +  io2 + cpu);
        }

        [HttpGet]
        public async Task<IHttpActionResult> ProfiledGoAsync()
        {
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

            MiniProfiler.Stop();
            Debug.WriteLine(MiniProfiler.Current.RenderPlainText());
            return Ok(io1 + io2 + cpu);
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

    }
}