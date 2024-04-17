using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Json;

// see https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0
public class DeliveryApiJsonTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Type == typeof(IApiContent))
        {
            ConfigureJsonPolymorphismOptions(jsonTypeInfo, typeof(ApiContent));
        }
        else if (jsonTypeInfo.Type == typeof(IApiContentResponse))
        {
            ConfigureJsonPolymorphismOptions(jsonTypeInfo, typeof(ApiContentResponse));
        }
        else if (jsonTypeInfo.Type == typeof(IRichTextElement))
        {
            ConfigureJsonPolymorphismOptions(jsonTypeInfo, typeof(RichTextRootElement), typeof(RichTextGenericElement), typeof(RichTextTextElement));
        }

        return jsonTypeInfo;
    }

    private void ConfigureJsonPolymorphismOptions(JsonTypeInfo jsonTypeInfo, params Type[] derivedTypes)
    {
        jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };

        foreach (Type derivedType in derivedTypes)
        {
            jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType));
        }
    }
}
