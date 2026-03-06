using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.PublishedContent
{
    /// <summary>
    /// Provide an abstract base class for <c>IPublishedContent</c> implementations.
    /// </summary>
    /// <remarks>This base class does which (a) consistently resolves and caches the URL, (b) provides an implementation
    /// for this[alias], and (c) provides basic content set management.</remarks>
    // TODO ELEMENTS: correct version for the obsolete message here
    [Obsolete("Please implement PublishableContentBase instead. Scheduled for removal in VXX")]
    public abstract class PublishedContentBase : PublishableContentBase, IPublishedContent
    {
        private readonly IVariationContextAccessor? _variationContextAccessor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PublishedContentBase" /> class.
        /// </summary>
        /// <param name="variationContextAccessor">The variation context accessor.</param>
        protected PublishedContentBase(IVariationContextAccessor? variationContextAccessor) => _variationContextAccessor = variationContextAccessor;

        /// <inheritdoc />
        /// <inheritdoc />
        public virtual string Name => this.Name(_variationContextAccessor);

        /// <inheritdoc />
        [Obsolete("Please use GetUrlSegment() on IDocumentUrlService instead. Scheduled for removal in Umbraco 18.")]
        public virtual string? UrlSegment => this.UrlSegment(_variationContextAccessor);

        /// <inheritdoc />
        [Obsolete("Not supported for members. Scheduled for removal in Umbraco 18.")]
        public abstract int Level { get; }

        /// <inheritdoc />
        [Obsolete("Not supported for members. Scheduled for removal in Umbraco 18.")]
        public abstract string Path { get; }

        /// <inheritdoc />
        public abstract int? TemplateId { get; }
    }
}
