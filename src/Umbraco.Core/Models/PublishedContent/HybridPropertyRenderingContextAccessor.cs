using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Implements a hybrid <see cref="IPropertyRenderingContextAccessor" />.
/// </summary>
public class HybridPropertyRenderingContextAccessor : HybridAccessorBase<PropertyRenderingContext>, IPropertyRenderingContextAccessor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HybridPropertyRenderingContextAccessor" /> class.
    /// </summary>
    /// <param name="requestCache">The request cache.</param>
    public HybridPropertyRenderingContextAccessor(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    /// <inheritdoc/>
    public PropertyRenderingContext? PropertyRenderingContext
    {
        get => Value;
        set => Value = value;
    }
}
