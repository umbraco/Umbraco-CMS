using Umbraco.Core.Cache;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models.PublishedContent
{
    /// <summary>
    /// Implements a hybrid <see cref="IVariationContextAccessor"/>.
    /// </summary>
    internal class HybridVariationContextAccessor : HybridAccessorBase<VariationContext>, IVariationContextAccessor
    {
        public HybridVariationContextAccessor(IRequestCache requestCache)
            : base(requestCache)
        { }

        /// <inheritdoc />
        protected override string ItemKey => "Umbraco.Web.HybridVariationContextAccessor";

        /// <summary>
        /// Gets or sets the <see cref="VariationContext"/> object.
        /// </summary>
        public VariationContext VariationContext
        {
            get => Value;
            set => Value = value;
        }
    }
}
