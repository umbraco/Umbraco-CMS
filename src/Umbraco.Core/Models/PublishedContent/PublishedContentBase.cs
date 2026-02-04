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
        [Obsolete("Please use GetUrlSegment() on IDocumentUrlService instead. Scheduled for removal in V16.")]
        public virtual string? UrlSegment => this.UrlSegment(_variationContextAccessor);

        /// <inheritdoc />
        [Obsolete("Not supported for members, scheduled for removal in v17")]
        public abstract int Level { get; }

        /// <inheritdoc />
        [Obsolete("Not supported for members, scheduled for removal in v17")]
        public abstract string Path { get; }

        /// <inheritdoc />
        public abstract int? TemplateId { get; }

        /// <inheritdoc />
        [Obsolete("Please use TryGetParentKey() on IDocumentNavigationQueryService or IMediaNavigationQueryService instead. Scheduled for removal in V16.")]
        public abstract IPublishedContent? Parent { get; }

        /// <inheritdoc />
        [Obsolete("Please use TryGetChildrenKeys() on IDocumentNavigationQueryService or IMediaNavigationQueryService instead. Scheduled for removal in V16.")]
        public virtual IEnumerable<IPublishedContent> Children => GetChildren();

        /// <summary>
        ///     Gets the children of the current content item.
        /// </summary>
        /// <returns>The children of the current content item.</returns>
        private IEnumerable<IPublishedContent> GetChildren()
        {
            INavigationQueryService? navigationQueryService;
            IPublishedStatusFilteringService? publishedStatusFilteringService;

            switch (ContentType.ItemType)
            {
                case PublishedItemType.Content:
                    navigationQueryService = StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>();
                    publishedStatusFilteringService = StaticServiceProvider.Instance.GetRequiredService<IPublishedContentStatusFilteringService>();
                    break;
                case PublishedItemType.Media:
                    navigationQueryService = StaticServiceProvider.Instance.GetRequiredService<IMediaNavigationQueryService>();
                    publishedStatusFilteringService = StaticServiceProvider.Instance.GetRequiredService<IPublishedMediaStatusFilteringService>();
                    break;
                default:
                    throw new NotImplementedException("Level is not implemented for " + ContentType.ItemType);
            }

            return this.Children(navigationQueryService, publishedStatusFilteringService);
        }
    }
}
