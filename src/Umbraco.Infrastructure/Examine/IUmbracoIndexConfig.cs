using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// Represents the configuration settings used to define and customize an Umbraco Examine index.
/// </summary>
public interface IUmbracoIndexConfig
{
    /// <summary>
    /// Gets the <see cref="Umbraco.Cms.Infrastructure.Examine.IContentValueSetValidator" /> used to validate content value sets before they are indexed.
    /// </summary>
    /// <returns>An instance of <see cref="Umbraco.Cms.Infrastructure.Examine.IContentValueSetValidator" /> for validating content value sets.</returns>
    IContentValueSetValidator GetContentValueSetValidator();

    /// <summary>Gets the validator used for published content value sets.</summary>
    /// <returns>An <see cref="IContentValueSetValidator"/> instance for validating published content value sets.</returns>
    IContentValueSetValidator GetPublishedContentValueSetValidator();

    /// <summary>
    /// Returns the <see cref="IValueSetValidator"/> used to validate value sets for members.
    /// </summary>
    /// <returns>The validator for member value sets.</returns>
    IValueSetValidator GetMemberValueSetValidator();
}
