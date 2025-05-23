using Umbraco.Cms.Api.Common.Serialization;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class SubTypesHandler : ISubTypesHandler
{
    private readonly IUmbracoJsonTypeInfoResolver _umbracoJsonTypeInfoResolver;

    public SubTypesHandler(IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver)
        => _umbracoJsonTypeInfoResolver = umbracoJsonTypeInfoResolver;

    protected virtual bool CanHandle(Type type)
        => type.Namespace?.StartsWith("Umbraco.Cms") is true;

    public virtual bool CanHandle(Type type, string documentName)
        => CanHandle(type);

    public virtual IEnumerable<Type> Handle(Type type)
        => _umbracoJsonTypeInfoResolver.FindSubTypes(type);
}
