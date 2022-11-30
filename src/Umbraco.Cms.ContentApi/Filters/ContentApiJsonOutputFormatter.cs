using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Umbraco.Cms.ContentApi.Filters;

public class ContentApiJsonOutputFormatter : SystemTextJsonOutputFormatter
{
    public ContentApiJsonOutputFormatter()
        : base(SerializerOptions())
    {
    }

    protected static JsonSerializerOptions SerializerOptions()
    {
        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new ContentApiJsonTypeResolver()
        };

        serializerOptions.Converters.Add(new JsonStringEnumConverter());

        return serializerOptions;
    }
}
