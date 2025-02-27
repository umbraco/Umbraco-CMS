using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Models.Context;

/// <summary>
///     Gives access to the current <see cref="BackOfficeVariationContext" />.
/// </summary>
public interface IBackOfficeVariationContextAccessor
{
    /// <summary>
    ///     Gets or sets the current <see cref="BackOfficeVariationContext" />.
    /// </summary>
    BackOfficeVariationContext? VariationContext { get; set; }
}
