using Umbraco.Cms.Web.UI;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureUmbracoDefaults()
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStaticWebAssets();
        webBuilder.UseStartup<Startup>();
    })
    .Build();

host.Run();
