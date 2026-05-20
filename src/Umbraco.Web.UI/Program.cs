#if UseDocumentedCsp
using Umbraco.Cms.Web.UI.Extensions;
#endif

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

//-:cnd:noEmit
#if DEBUG
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
#endif
//+:cnd:noEmit

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
#if UseDeliveryApi
    .AddDeliveryApi()
#endif
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

#if UseDocumentedCsp
app.UseDocumentedContentSecurityPolicy();
#endif

await app.BootUmbracoAsync();

#if UseHttpsRedirect
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
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
