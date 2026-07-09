#if UseDocumentedCsp
using Umbraco.Cms.Web.UI.Extensions;
#endif

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
