using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.ManagementApi.Filters;

public class UmbracoSystemTextJsonModelBinder : BodyModelBinder
{
    public UmbracoSystemTextJsonModelBinder(
        IHttpRequestStreamReaderFactory readerFactory,
        ILoggerFactory loggerFactory)
        : base(GetSystemTextJsonFormatter(loggerFactory), readerFactory, loggerFactory)
    {
    }

    private static IInputFormatter[] GetSystemTextJsonFormatter(ILoggerFactory logger)
    {
        var jsonOptions = new JsonOptions { JsonSerializerOptions = { Converters = { new JsonStringEnumConverter() } } };

        return new IInputFormatter[]
        {
            new SystemTextJsonInputFormatter(jsonOptions, logger.CreateLogger<SystemTextJsonInputFormatter>()),
        };
    }
}
