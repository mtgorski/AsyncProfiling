using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AsyncProfilingDemo.Web
{
    public class CpuService : ICpuService
    {
        public virtual int Compute()
        {
            Task.Delay(125).GetAwaiter().GetResult();
            return 77;
        }
    }

    public interface ICpuService
    {
        int Compute();
    }
}