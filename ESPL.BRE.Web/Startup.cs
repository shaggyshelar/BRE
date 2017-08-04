using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ESPL.BRE.Web.Startup))]
namespace ESPL.BRE.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
