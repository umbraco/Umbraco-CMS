using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.PublishedContent
{
    /// <summary>
    /// Provide an abstract base class for <c>IPublishedContent</c> implementations.
    /// </summary>
    /// <remarks>This base class does which (a) consistently resolves and caches the URL, (b) provides an implementation
    /// for this[alias], and (c) provides basic content set management.</remarks>
    [DebuggerDisplay("Content Id: {Id}")]
    public abstract class PublishedContentBase : IPublishedContent
    {
        private readonly IVariationContextAccessor? _variationContextAccessor;

        protected PublishedContentBase(IVariationContextAccessor? variationContextAccessor) => _variationContextAccessor = variationContextAccessor;

        public abstract IPublishedContentType ContentType { get; }

        /// <inheritdoc />
        public abstract Guid Key { get; }

        /// <inheritdoc />
        public abstract int Id { get; }

        /// <inheritdoc />
        public virtual string Name => this.Name(_variationContextAccessor);

        /// <inheritdoc />
        [Obsolete("Please use GetUrlSegment() on IDocumentUrlService instead. Scheduled for removal in V16.")]
        public virtual string? UrlSegment => this.UrlSegment(_variationContextAccessor);

        /// <inheritdoc />
        public abstract int SortOrder { get; }

        /// <inheritdoc />
        [Obsolete("Not supported for members, scheduled for removal in v17")]
        public abstract int Level { get; }

        /// <inheritdoc />
        [Obsolete("Not supported for members, scheduled for removal in v17")]
        public abstract string Path { get; }

        /// <inheritdoc />
        public abstract int? TemplateId { get; }

        /// <inheritdoc />
        public abstract int CreatorId { get; }

        /// <inheritdoc />
        public abstract DateTime CreateDate { get; }

        /// <inheritdoc />
        public abstract int WriterId { get; }

        /// <inheritdoc />
        public abstract DateTime UpdateDate { get; }

        /// <inheritdoc />
        public abstract IReadOnlyDictionary<string, PublishedCultureInfo> Cultures { get; }

        /// <inheritdoc />
        public abstract PublishedItemType ItemType { get; }

        /// <inheritdoc />
        public abstract bool IsDraft(string? culture = null);

        /// <inheritdoc />
        public abstract bool IsPublished(string? culture = null);

        /// <inheritdoc />
        [Obsolete("Please use TryGetParentKey() on IDocumentNavigationQueryService or IMediaNavigationQueryService instead. Scheduled for removal in V16.")]
        public abstract IPublishedContent? Parent { get; }

        /// <inheritdoc />
        [Obsolete("Please use TryGetChildrenKeys() on IDocumentNavigationQueryService or IMediaNavigationQueryService instead. Scheduled for removal in V16.")]
        public virtual IEnumerable<IPublishedContent> Children => GetChildren();


        /// <inheritdoc cref="IPublishedElement.Properties"/>
        public abstract IEnumerable<IPublishedProperty> Properties { get; }

        /// <inheritdoc cref="IPublishedElement.GetProperty(string)"/>
        public abstract IPublishedProperty? GetProperty(string alias);

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
