using Umbraco.Cms.Api.Common.Serialization;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class SubTypesHandler(IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver) : ISubTypesHandler
{
    public virtual bool CanHandle(Type type)
        => type.Namespace?.StartsWith("Umbraco.Cms") is true;

    public virtual bool CanHandle(Type type, string documentName)
        => CanHandle(type);

    public virtual IEnumerable<Type> Handle(Type type)
        => umbracoJsonTypeInfoResolver.FindSubTypes(type);
}
