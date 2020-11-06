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
                // TODO: MSDI - this should probably be on one day, more so when we can reduce the number
                // of times we build a ServiceProvider from services collection
                // right now it's just painful.
                .UseDefaultServiceProvider(options => options.ValidateOnBuild = false)
                .UseUmbraco();
    }
}
