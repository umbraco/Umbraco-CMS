using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace Segments.Features.Segments
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class SegmentsComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<IVariationContextAccessor, SegmentVariationContextAccessor>();
        }
    }

    public class SegmentVariationContextAccessor : HybridAccessorBase<VariationContext>, IVariationContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SegmentVariationContextAccessor(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        protected override string ItemKey => "Umbraco.Web.HybridVariationContextAccessor";

        /// <summary>
        /// Gets or sets the <see cref="VariationContext"/> object.
        /// </summary>
        public VariationContext VariationContext
        {
            get
            {
                var defaultContext = Value;

                if (_httpContextAccessor.HttpContext?.Request?.QueryString["s"] is string segment)
                {
                    return new VariationContext(culture: defaultContext?.Culture, segment: segment);
                }
                else
                {
                    return defaultContext;
                }
            }

            set
            {
                Value = value;
            }
        }
    }
}
