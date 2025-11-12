using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Infrastructure.Serialization;

public abstract class ContentJsonTypeResolverBase : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        Type[] derivedTypes = GetDerivedTypes(jsonTypeInfo);
        if (derivedTypes.Length > 0)
        {
            ConfigureJsonPolymorphismOptions(jsonTypeInfo, derivedTypes);
        }

        return jsonTypeInfo;
    }

    public virtual Type[] GetDerivedTypes(JsonTypeInfo jsonTypeInfo) =>
        jsonTypeInfo.Type switch
        {
            _ when jsonTypeInfo.Type == typeof(IApiMediaWithCropsResponse) => [typeof(ApiMediaWithCropsResponse)],
            _ when jsonTypeInfo.Type == typeof(IApiMediaWithCrops) => [typeof(ApiMediaWithCrops)],
            _ => [],
        };

    public void ConfigureJsonPolymorphismOptions(JsonTypeInfo jsonTypeInfo, params Type[] derivedTypes)
    {
        jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions();

        foreach (Type derivedType in derivedTypes)
        {
            jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType));
        }
    }
}
