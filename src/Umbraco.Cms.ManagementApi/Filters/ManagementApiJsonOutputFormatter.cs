using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Umbraco.Cms.ManagementApi.Filters;

public class ManagementApiJsonOutputFormatter : SystemTextJsonOutputFormatter
{
    public ManagementApiJsonOutputFormatter(JsonSerializerOptions jsonSerializerOptions) : base(RegisterJsonConverters(jsonSerializerOptions))
    {
    }

    protected static JsonSerializerOptions RegisterJsonConverters(JsonSerializerOptions serializerOptions)
    {
        serializerOptions.Converters.Add(new JsonStringEnumConverter());

        return serializerOptions;
    }
}
