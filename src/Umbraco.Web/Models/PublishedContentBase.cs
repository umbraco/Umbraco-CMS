using System;
using System.Collections.Generic;
using System.Diagnostics;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// Provide an abstract base class for <c>IPublishedContent</c> implementations.
    /// </summary>
    /// <remarks>This base class does which (a) consistently resolves and caches the URL, (b) provides an implementation
    /// for this[alias], and (c) provides basic content set management.</remarks>
    [DebuggerDisplay("Content Id: {Id}")]
    public abstract class PublishedContentBase : IPublishedContent
    {
        #region ContentType

        public abstract IPublishedContentType ContentType { get; }

        #endregion

        #region PublishedElement

        /// <inheritdoc />
        public abstract Guid Key { get; }

        #endregion

        #region PublishedContent

        /// <inheritdoc />
        public abstract int Id { get; }

        /// <inheritdoc />
        public virtual string Name => this.Name();

        /// <inheritdoc />
        public virtual string UrlSegment => this.UrlSegment();

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
        [Obsolete("Use CreatorName(IUserService) extension instead")]
        public abstract string CreatorName { get; }

        /// <inheritdoc />
        public abstract DateTime CreateDate { get; }

        /// <inheritdoc />
        public abstract int WriterId { get; }

        /// <inheritdoc />
        [Obsolete("Use WriterName(IUserService) extension instead")]
        public abstract string WriterName { get; }

        /// <inheritdoc />
        public abstract DateTime UpdateDate { get; }

        /// <inheritdoc />
        [Obsolete("Use the Url() extension instead")]
        public virtual string Url => this.Url();

        /// <inheritdoc />
        public abstract IReadOnlyDictionary<string, PublishedCultureInfo> Cultures { get; }

        /// <inheritdoc />
        public abstract PublishedItemType ItemType { get; }

        /// <inheritdoc />
        public abstract bool IsDraft(string culture = null);

        /// <inheritdoc />
        public abstract bool IsPublished(string culture = null);

        #endregion

        #region Tree

        /// <inheritdoc />
        public abstract IPublishedContent Parent { get; }

        /// <inheritdoc />
        public virtual IEnumerable<IPublishedContent> Children => this.Children();

        /// <inheritdoc />
        public abstract IEnumerable<IPublishedContent> ChildrenForAllCultures { get; }

        #endregion

        #region Properties

        /// <inheritdoc cref="IPublishedElement.Properties"/>
        public abstract IEnumerable<IPublishedProperty> Properties { get; }

        /// <inheritdoc cref="IPublishedElement.GetProperty(string)"/>
        public abstract IPublishedProperty GetProperty(string alias);

        #endregion
    }
}
