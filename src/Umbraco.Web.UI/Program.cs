#if (MinimalHostingModel)
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.ConfigureUmbracoDefaults();
builder.Services.AddUmbraco(builder.Environment, builder.Configuration)
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

#if (UseHttpsRedirect)

    app.UseHttpsRedirection();
#endif

app.UseUmbraco()
    .WithMiddleware(u =>
    {
      u.UseBackOffice();
      u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
      u.UseInstallerEndpoints();
      u.UseBackOfficeEndpoints();
      u.UseWebsiteEndpoints();
    });

app.Run();
#else
namespace Umbraco.Cms.Web.UI
{
    public class Program
    {
        public static void Main(string[] args)
            => CreateHostBuilder(args)
                .Build()
                .Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureUmbracoDefaults()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStaticWebAssets();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
#endif
