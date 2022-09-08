namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Gives access to the current <see cref="VariationContext" />.
/// </summary>
public interface IVariationContextAccessor
{
    /// <summary>
    ///     Gets or sets the current <see cref="VariationContext" />.
    /// </summary>
    VariationContext? VariationContext { get; set; }
}
