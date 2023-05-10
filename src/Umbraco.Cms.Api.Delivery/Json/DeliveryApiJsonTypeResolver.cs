using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Json;

// see https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0
// TODO: if this type resolver is to be used for extendable content models (custom IApiContent implementations) we need to work out an extension model for known derived types
internal sealed class DeliveryApiJsonTypeResolver : DefaultJsonTypeInfoResolver
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
            ConfigureJsonPolymorphismOptions(jsonTypeInfo, typeof(RichTextGenericElement), typeof(RichTextTextElement));
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
