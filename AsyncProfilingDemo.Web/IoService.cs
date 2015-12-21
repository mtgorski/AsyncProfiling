using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AsyncProfilingDemo.Web
{
    public class IoService : IIoService
    {

        public async Task<int> OperationOneAsync()
        {
            await Task.Delay(300);
            return 13;
        }

        public async Task<int> OperationTwoAsync()
        {
            await Task.Delay(3000);
            return 10;
        }
    }

    public interface IIoService
    {
        Task<int> OperationOneAsync();
        Task<int> OperationTwoAsync();
    }

}