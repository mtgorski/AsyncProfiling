using Ninject.Extensions.Interception;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace AsyncProfilingDemo.Web
{
    public class ProfilingInterceptor : IInterceptor
    {
        private IDisposable step;
        private MiniProfiler _currentProfiler;
        private static MethodInfo startTaskMethodInfo = typeof(ProfilingInterceptor).GetMethod("InvokeWithResultAsync", BindingFlags.Instance | BindingFlags.NonPublic);

        public ProfilingInterceptor(MiniProfiler currentProfiler) 
        {
            _currentProfiler = currentProfiler;
        }

        private void BeforeInvoke(IInvocation invocation)
        {
            step = _currentProfiler.Step(invocation.Request.Target.GetType().Name + "_" +  invocation.Request.Method.Name);
        }

        private void AfterInvoke(IInvocation invocation)
        {
            step.Dispose();
        }

        public void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Request.Method.ReturnType;
            BeforeInvoke(invocation);
            invocation.Proceed();
            if(invocation.Request.Method.ReturnType == typeof(Task))
            {
                invocation.ReturnValue = InvokeAsync(invocation, (Task)invocation.ReturnValue);
            }
            else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var resultType = returnType.GetGenericArguments()[0];
                var method = startTaskMethodInfo.MakeGenericMethod(resultType);
                invocation.ReturnValue = method.Invoke(this, new[]{ invocation, invocation.ReturnValue});
            }
            else
            {
                AfterInvoke(invocation);
            }
        }

        private async Task InvokeAsync(IInvocation invocation, Task originalTask)
        {
            await originalTask;
            AfterInvoke(invocation);
        }

        private async Task<T> InvokeWithResultAsync<T>(IInvocation invocation, Task<T> originalTask)
        {
            var returnVal = await originalTask;
            AfterInvoke(invocation);
            return returnVal;
        }
    }

    public class SharedProfilingInterceptor : IInterceptor
    {

        public void Intercept(IInvocation invocation)
        {
            var interceptor = new ProfilingInterceptor(MiniProfiler.Current);
            interceptor.Intercept(invocation);
        }
    }
}