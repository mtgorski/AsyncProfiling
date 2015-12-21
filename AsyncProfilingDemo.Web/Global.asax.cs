using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace AsyncProfilingDemo.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static readonly bool ProfilingEnabled = bool.Parse(ConfigurationManager.AppSettings["UseMiniProfilerInterceptor"]);

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            if(ProfilingEnabled)
            {
                GlobalConfiguration.Configuration.MessageHandlers.Add(new ProfilingStorageHandler());
            }
        }

        protected void Application_BeginRequest()
        {
            if (ProfilingEnabled)
            {
                MiniProfiler.Start();
                MiniProfiler.Current.Storage = new HttpRuntimeCacheStorage(TimeSpan.FromHours(1));
            } 
        }

        protected void Application_EndRequest()
        {
            MiniProfiler.Stop();
        }
    }
}
