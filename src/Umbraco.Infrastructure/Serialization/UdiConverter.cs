using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Serialization
{
    public class UdiConverter : JsonConverter<Udi>
    {
        public override bool CanConvert(Type typeToConvert) => typeof(Udi).IsAssignableFrom(typeToConvert);

        public override Udi? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                JsonElement json = JsonElement.ParseValue(ref reader);
                Udi udi = UdiParser.Parse(json.GetString()!);
                return udi;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, Udi value, JsonSerializerOptions options)
                => writer.WriteStringValue(value?.ToString());
    }
}
