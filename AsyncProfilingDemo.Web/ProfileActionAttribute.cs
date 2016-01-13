using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Filters;

namespace AsyncProfilingDemo.Web
{
    public class ProfileAction : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var step = MiniProfiler.Current.Step(actionContext.ActionDescriptor.ControllerDescriptor.ControllerName + "_" +  actionContext.ActionDescriptor.ActionName);
            actionContext.ActionArguments["profiling_step"] = step;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            (actionExecutedContext.ActionContext.ActionArguments["profiling_step"] as IDisposable).Dispose();
        }
    }
}