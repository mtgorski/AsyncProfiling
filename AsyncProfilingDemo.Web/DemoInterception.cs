using Ninject.Extensions.Interception;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace AsyncProfilingDemo.Web
{
    public class DemoInterception : IDemoInterception
    {
        public virtual int GetResult()
        {
            return 10;
        }

    }

    public interface IDemoInterception
    {
        int GetResult();
    }

    public class ModifyingInterceptor : IInterceptor
    {

        public void Intercept(IInvocation invocation)
        {
            Debug.WriteLine("Start interception");
            invocation.Proceed();
            invocation.ReturnValue = (int)invocation.ReturnValue + 33;
            Debug.WriteLine("Finished interception.");
        }
    }
}