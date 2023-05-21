using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Web.Common.Formatters;

/// <summary>
///     This will format the JSON output for use with AngularJs's approach to JSON Vulnerability attacks
/// </summary>
/// <remarks>
///     See: http://docs.angularjs.org/api/ng.$http (Security considerations)
/// </remarks>
public class AngularJsonMediaTypeFormatter : NewtonsoftJsonOutputFormatter
{
    public const string XsrfPrefix = ")]}',\n";

    public AngularJsonMediaTypeFormatter(JsonSerializerSettings serializerSettings, ArrayPool<char> charPool, MvcOptions mvcOptions)
        : base(RegisterJsonConverters(serializerSettings), charPool, mvcOptions)
    {
    }

    protected static JsonSerializerSettings RegisterJsonConverters(JsonSerializerSettings serializerSettings)
    {
        serializerSettings.Converters.Add(new StringEnumConverter());
        serializerSettings.Converters.Add(new UdiJsonConverter());

        return serializerSettings;
    }

    protected override JsonWriter CreateJsonWriter(TextWriter writer)
    {
        JsonWriter jsonWriter = base.CreateJsonWriter(writer);

        jsonWriter.WriteRaw(XsrfPrefix);

        return jsonWriter;
    }
}
