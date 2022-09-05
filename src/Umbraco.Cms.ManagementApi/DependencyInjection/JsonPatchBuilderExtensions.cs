using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class JsonPatchBuilderExtensions
{
    internal static IUmbracoBuilder AddJsonPatch(this IUmbracoBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
        });
        return builder;
    }

    private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
    {
        ServiceProvider? builder = new ServiceCollection()
            .AddLogging()
            .AddMvc()
            .AddNewtonsoftJson()
            .Services.BuildServiceProvider();

        return builder
            .GetRequiredService<IOptions<MvcOptions>>()
            .Value
            .InputFormatters
            .OfType<NewtonsoftJsonPatchInputFormatter>()
            .First();
    }
}
