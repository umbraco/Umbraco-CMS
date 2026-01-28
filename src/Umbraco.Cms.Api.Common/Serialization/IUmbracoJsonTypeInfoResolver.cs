using System.Text.Json.Serialization.Metadata;

namespace Umbraco.Cms.Api.Common.Serialization;

/// <summary>
///     Extends <see cref="IJsonTypeInfoResolver"/> with Umbraco-specific type resolution for polymorphic JSON serialization.
/// </summary>
public interface IUmbracoJsonTypeInfoResolver : IJsonTypeInfoResolver
{
    /// <summary>
    ///     Finds all sub-types of the specified type for polymorphic serialization.
    /// </summary>
    /// <param name="type">The base type to find sub-types for.</param>
    /// <returns>An enumerable of sub-types.</returns>
    IEnumerable<Type> FindSubTypes(Type type);

    /// <summary>
    ///     Gets the type discriminator value used for polymorphic serialization.
    /// </summary>
    /// <param name="type">The type to get the discriminator value for.</param>
    /// <returns>The discriminator value, or <c>null</c> if not applicable.</returns>
    string? GetTypeDiscriminatorValue(Type type);
}
