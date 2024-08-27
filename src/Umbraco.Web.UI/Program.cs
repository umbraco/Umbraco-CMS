WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(opt =>
    opt.AddPolicy("AllowAll", options => options.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod()));

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
#if UseDeliveryApi
    .AddDeliveryApi()
#endif
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

#if (UseHttpsRedirect)
app.UseHttpsRedirection();
#endif

app.UseCors("AllowAll");

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        /*#if (UmbracoRelease = 'LTS')
        u.UseInstallerEndpoints();
        #endif */
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
