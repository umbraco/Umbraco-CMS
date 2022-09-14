using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Umbraco.Cms.ManagementApi.Filters;

public class EnumOutputFormatter : SystemTextJsonOutputFormatter
{
    public EnumOutputFormatter(JsonSerializerOptions jsonSerializerOptions) : base(RegisterJsonConverters(jsonSerializerOptions))
    {
    }

    protected static JsonSerializerOptions RegisterJsonConverters(JsonSerializerOptions serializerOptions)
    {
        serializerOptions.Converters.Add(new JsonStringEnumConverter());

        return serializerOptions;
    }
}
