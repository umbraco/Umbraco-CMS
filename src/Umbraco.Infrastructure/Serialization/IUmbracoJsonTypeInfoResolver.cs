using System.Text.Json.Serialization.Metadata;

namespace Umbraco.Cms.Infrastructure.Serialization;

public interface IUmbracoJsonTypeInfoResolver : IJsonTypeInfoResolver
{
    IEnumerable<Type> FindSubTypes(Type type);
}
