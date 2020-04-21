using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Umbraco.Core.Composing;
using Umbraco.Web.Common.Extensions;

namespace Umbraco.Web.UI.BackOffice
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
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .UseUmbraco();
    }
}
