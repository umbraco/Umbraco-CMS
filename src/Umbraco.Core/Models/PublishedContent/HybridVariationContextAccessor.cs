using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Implements a hybrid <see cref="IVariationContextAccessor" />.
/// </summary>
public class HybridVariationContextAccessor : HybridAccessorBase<VariationContext>, IVariationContextAccessor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HybridVariationContextAccessor"/> class.
    /// </summary>
    /// <param name="requestCache">The request cache.</param>
    public HybridVariationContextAccessor(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    /// <summary>
    ///     Gets or sets the <see cref="VariationContext" /> object.
    /// </summary>
    public VariationContext? VariationContext
    {
        get => Value;
        set => Value = value;
    }
}
