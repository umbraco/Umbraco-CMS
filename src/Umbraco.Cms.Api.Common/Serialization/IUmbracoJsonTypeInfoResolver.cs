using System.Text.Json.Serialization.Metadata;

namespace Umbraco.Cms.Api.Common.Serialization;

public interface IUmbracoJsonTypeInfoResolver : IJsonTypeInfoResolver
{
    IEnumerable<Type> FindSubTypes(Type type);

    string? GetTypeDiscriminatorValue(Type type);
}
