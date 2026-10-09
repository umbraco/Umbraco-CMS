namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Gives access to the current <see cref="PropertyRenderingContext" />.
/// </summary>
public interface IPropertyRenderingContextAccessor
{
    /// <summary>
    ///     Gets or sets the current <see cref="PropertyRenderingContext" />.
    /// </summary>
    PropertyRenderingContext? PropertyRenderingContext { get; set; }
}
