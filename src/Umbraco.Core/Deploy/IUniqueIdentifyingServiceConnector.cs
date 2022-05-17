namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Provides a method to retrieve an artifact's unique identifier.
/// </summary>
/// <remarks>
///     Artifacts are uniquely identified by their <see cref="Udi" />, however they represent
///     elements in Umbraco that may be uniquely identified by another value. For example,
///     a content type is uniquely identified by its alias. If someone creates a new content
///     type, and tries to deploy it to a remote environment where a content type with the
///     same alias already exists, both content types end up having different <see cref="Udi" />
///     but the same alias. By default, Deploy would fail and throw when trying to save the
///     new content type (duplicate alias). However, if the connector also implements this
///     interface, the situation can be detected beforehand and reported in a nicer way.
/// </remarks>
public interface IUniqueIdentifyingServiceConnector
{
    /// <summary>
    ///     Gets the unique identifier of the specified artifact.
    /// </summary>
    /// <param name="artifact">The artifact.</param>
    /// <returns>The unique identifier.</returns>
    string GetUniqueIdentifier(IArtifact artifact);
}
