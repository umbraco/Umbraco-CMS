using Umbraco.Cms.Api.Common.Serialization;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
///     Default handler for discovering sub-types for polymorphic OpenAPI schemas.
/// </summary>
public class SubTypesHandler : ISubTypesHandler
{
    private readonly IUmbracoJsonTypeInfoResolver _umbracoJsonTypeInfoResolver;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SubTypesHandler"/> class.
    /// </summary>
    /// <param name="umbracoJsonTypeInfoResolver">The JSON type info resolver for finding sub-types.</param>
    public SubTypesHandler(IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver)
        => _umbracoJsonTypeInfoResolver = umbracoJsonTypeInfoResolver;

    /// <summary>
    ///     Determines whether this handler can process the specified type based on namespace.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is in an Umbraco.Cms namespace; otherwise, <c>false</c>.</returns>
    protected virtual bool CanHandle(Type type)
        => type.Namespace?.StartsWith("Umbraco.Cms") is true;

    /// <inheritdoc/>
    public virtual bool CanHandle(Type type, string documentName)
        => CanHandle(type);

    /// <inheritdoc/>
    public virtual IEnumerable<Type> Handle(Type type)
        => _umbracoJsonTypeInfoResolver.FindSubTypes(type);
}
