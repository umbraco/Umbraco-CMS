#if false
// This file is shared with the `dotnet new umbraco` template. Wrap any #if directive that isn't a
// declared template parameter (see templates/UmbracoProject/.template.config/template.json) with
// //-:cnd:noEmit / //+:cnd:noEmit so the template engine emits it verbatim instead of stripping it.
#endif
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
