using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AsyncProfilingDemo.MVC.Startup))]
namespace AsyncProfilingDemo.MVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
