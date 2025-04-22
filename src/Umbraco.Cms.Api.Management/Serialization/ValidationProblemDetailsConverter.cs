using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Serialization;


public sealed class ValidationProblemDetailsDetailsConverter : JsonConverter<ValidationProblemDetails>
{
    /// <inheritdoc />
    public override ValidationProblemDetails? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => null;

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ValidationProblemDetails value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);
        writer.WriteString("title", value.Title);

        if (value.Status.HasValue)
        {
            writer.WriteNumber("status", value.Status.Value);
        }

        if (value.Detail is not null)
        {
            writer.WriteString("detail", value.Detail);
        }

        if (value.Instance is not null)
        {
            writer.WriteString("instance", value.Instance);
        }

        if (value.Errors.Any())
        {
            writer.WritePropertyName("errors");
            writer.WriteStartArray();

            foreach (KeyValuePair<string, string[]> error in value.Errors)
            {
                writer.WriteStartObject();
                writer.WritePropertyName($"$.{string.Join(".", error.Key.Split(Constants.CharArrays.Period).Select(s => s.ToFirstLowerInvariant()))}");
                writer.WriteStartArray();

                foreach (var errorDetails in error.Value)
                {
                    writer.WriteStringValue(errorDetails);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        if (value.Extensions.TryGetValue("traceId", out var traceId) && traceId is string traceIdValue)
        {
            writer.WriteString("traceId", traceIdValue);
        }

        writer.WriteEndObject();
    }
}
