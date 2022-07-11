using System.Diagnostics;
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
        public virtual string? Name => this.Name(_variationContextAccessor);

        /// <inheritdoc />
        public virtual string? UrlSegment => this.UrlSegment(_variationContextAccessor);

        /// <inheritdoc />
        public abstract int SortOrder { get; }

        /// <inheritdoc />
        public abstract int Level { get; }

        /// <inheritdoc />
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
        public abstract IPublishedContent? Parent { get; }

        /// <inheritdoc />
        public virtual IEnumerable<IPublishedContent>? Children => this.Children(_variationContextAccessor);

        /// <inheritdoc />
        public abstract IEnumerable<IPublishedContent> ChildrenForAllCultures { get; }

        /// <inheritdoc cref="IPublishedElement.Properties"/>
        public abstract IEnumerable<IPublishedProperty> Properties { get; }

        /// <inheritdoc cref="IPublishedElement.GetProperty(string)"/>
        public abstract IPublishedProperty? GetProperty(string alias);
    }
}
