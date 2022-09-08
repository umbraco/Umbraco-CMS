using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a content app factory.
/// </summary>
public interface IContentAppFactory
{
    /// <summary>
    ///     Gets the content app for an object.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="userGroups">The user groups</param>
    /// <returns>The content app for the object, or null.</returns>
    /// <remarks>
    ///     <para>
    ///         The definition must determine, based on <paramref name="source" />, whether
    ///         the content app should be displayed or not, and return either a <see cref="ContentApp" />
    ///         instance, or null.
    ///     </para>
    /// </remarks>
    ContentApp? GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups);
}
