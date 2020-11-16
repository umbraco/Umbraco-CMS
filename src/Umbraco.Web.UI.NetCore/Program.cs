using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Composing;

namespace Umbraco.Web.UI.NetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(x =>
                {
                    x.ClearProviders();
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                // TODO: MSDI - this should probably be on one day
                // First we need to resolve the composition conditional registration issues see #8563
                .UseDefaultServiceProvider(options => options.ValidateOnBuild = false)
                .UseUmbraco();
    }
}
