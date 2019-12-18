using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models.PublishedContent
{
    /// <summary>
    /// Implements <see cref="IVariationContextAccessor"/> on top of <see cref="IHttpContextAccessor"/>.
    /// </summary>
    public class HttpContextVariationContextAccessor : IVariationContextAccessor
    {
        public const string ContextKey = "Umbraco.Web.Models.PublishedContent.DefaultVariationContextAccessor";
        public readonly IHttpContextAccessor HttpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextVariationContextAccessor"/> class.
        /// </summary>
        public HttpContextVariationContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public VariationContext VariationContext
        {
            get => (VariationContext) HttpContextAccessor.HttpContext?.Items[ContextKey];
            set => HttpContextAccessor.HttpContext.Items[ContextKey] = value;
        }
    }
}
