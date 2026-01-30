using Umbraco.Cms.Core.Security;

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

app.Use(async (context, next) =>
{
    ICspNonceService cspNonceService = context.RequestServices.GetRequiredService<ICspNonceService>();
    var nonce = cspNonceService.GetNonce();

    context.Response.Headers.Append("Content-Security-Policy",
        $"default-src 'self'; " +
        $"script-src 'self' 'nonce-{nonce}'; " +
        $"style-src 'self' 'unsafe-inline'; " +
        $"img-src 'self' data: news-dashboard.umbraco.com; " +
        $"connect-src 'self'; " +
        $"font-src 'self'; " +
        $"frame-src 'self'");

    await next();
});

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
