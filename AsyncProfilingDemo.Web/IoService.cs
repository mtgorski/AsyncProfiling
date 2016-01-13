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

        public async Task ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new HttpException("It's as if I tried calling another service and it didn't work!");
        }
    }

    public interface IIoService
    {
        Task<int> OperationOneAsync();
        Task<int> OperationTwoAsync();
        Task ThrowExceptionAsync();
    }

}