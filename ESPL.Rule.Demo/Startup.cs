using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ESPL.Rule.Demo.Startup))]
namespace ESPL.Rule.Demo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
