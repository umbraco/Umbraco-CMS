namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <inheritdoc />
/// <summary>
///     Indicates that the class is a published content model for a specified content type.
/// </summary>
/// <remarks>
///     By default, the name of the class is assumed to be the content type alias. The
///     <c>PublishedContentModelAttribute</c> can be used to indicate a different alias.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PublishedModelAttribute : Attribute
{
    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedModelAttribute" /> class with a content type alias.
    /// </summary>
    /// <param name="contentTypeAlias">The content type alias.</param>
    public PublishedModelAttribute(string contentTypeAlias)
    {
        if (contentTypeAlias == null)
        {
            throw new ArgumentNullException(nameof(contentTypeAlias));
        }

        if (string.IsNullOrWhiteSpace(contentTypeAlias))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(contentTypeAlias));
        }

        ContentTypeAlias = contentTypeAlias;
    }

    /// <summary>
    ///     Gets or sets the content type alias.
    /// </summary>
    public string ContentTypeAlias { get; }
}
