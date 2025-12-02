using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.POC.JsonSchemaGeneration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

builder.Services.AddSingleton<JsonSchemaExporterService>();

WebApplication app = builder.Build();

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

app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head>
    <title>JSON Schema Endpoints</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        h1 { color: #333; }
        ul { list-style-type: none; padding: 0; }
        li { margin: 10px 0; }
        a { color: #0066cc; text-decoration: none; font-size: 18px; }
        a:hover { text-decoration: underline; }
    </style>
</head>
<body>
    <h1>JSON Schema Endpoints</h1>
    <ul>
        <li><a href=""/json/content"">Content Schema</a></li>
        <li><a href=""/json/media"">Media Schema</a></li>
    </ul>
</body>
</html>");
});
app.MapGet("/json/content",
    ([FromServices] JsonSchemaExporterService exporterService) =>
        Results.Content(exporterService.GenerateJsonSchema<IApiContentResponse>().ToJson(), "application/json"));
app.MapGet("/json/media",
    ([FromServices] JsonSchemaExporterService exporterService) =>
        Results.Content(exporterService.GenerateJsonSchema<IApiMediaWithCropsResponse>().ToJson(), "application/json"));

await app.RunAsync();
