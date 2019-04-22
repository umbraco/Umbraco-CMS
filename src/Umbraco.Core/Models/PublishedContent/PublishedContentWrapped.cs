using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.PublishedContent
{
    //
    // we cannot implement strongly-typed content by inheriting from some sort
    // of "master content" because that master content depends on the actual content cache
    // that is being used. It can be an XmlPublishedContent with the XmlPublishedCache,
    // or just anything else.
    //
    // So we implement strongly-typed content by encapsulating whatever content is
    // returned by the content cache, and providing extra properties (mostly) or
    // methods or whatever. This class provides the base for such encapsulation.
    //

    /// <summary>
    /// Provides an abstract base class for <c>IPublishedContent</c> implementations that
    /// wrap and extend another <c>IPublishedContent</c>.
    /// </summary>
    public abstract class PublishedContentWrapped : IPublishedContent
    {
        private readonly IPublishedContent _content;

        /// <summary>
        /// Initialize a new instance of the <see cref="PublishedContentWrapped"/> class
        /// with an <c>IPublishedContent</c> instance to wrap.
        /// </summary>
        /// <param name="content">The content to wrap.</param>
        protected PublishedContentWrapped(IPublishedContent content)
        {
            _content = content;
        }

        /// <summary>
        /// Gets the wrapped content.
        /// </summary>
        /// <returns>The wrapped content, that was passed as an argument to the constructor.</returns>
        public IPublishedContent Unwrap() => _content;

        #region ContentType

        /// <inheritdoc />
        public virtual IPublishedContentType ContentType => _content.ContentType;

        #endregion

        #region PublishedElement

        /// <inheritdoc />
        public Guid Key => _content.Key;

        #endregion

        #region PublishedContent

        /// <inheritdoc />
        public virtual int Id => _content.Id;

        /// <inheritdoc />
        public virtual string Name(string culture = null) => _content.Name(culture);

        /// <inheritdoc />
        public virtual string UrlSegment(string culture = null) => _content.UrlSegment(culture);

        /// <inheritdoc />
        public virtual int SortOrder => _content.SortOrder;

        /// <inheritdoc />
        public virtual int Level => _content.Level;

        /// <inheritdoc />
        public virtual string Path => _content.Path;

        /// <inheritdoc />
        public virtual int? TemplateId => _content.TemplateId;

        /// <inheritdoc />
        public virtual int CreatorId => _content.CreatorId;

        /// <inheritdoc />
        public virtual string CreatorName => _content.CreatorName;

        /// <inheritdoc />
        public virtual DateTime CreateDate => _content.CreateDate;

        /// <inheritdoc />
        public virtual int WriterId => _content.WriterId;

        /// <inheritdoc />
        public virtual string WriterName => _content.WriterName;

        /// <inheritdoc />
        public virtual DateTime UpdateDate => _content.UpdateDate;

        /// <inheritdoc />
        public virtual string Url(string culture = null, UrlMode mode = UrlMode.Auto) => _content.Url(culture, mode);

        /// <inheritdoc />
        public DateTime CultureDate(string culture = null) => _content.CultureDate(culture);

        /// <inheritdoc />
        public IReadOnlyList<string> Cultures => _content.Cultures;

        /// <inheritdoc />
        public virtual bool IsDraft(string culture = null) => _content.IsDraft(culture);

        /// <inheritdoc />
        public virtual bool IsPublished(string culture = null) => _content.IsPublished(culture);

        #endregion

        #region Tree

        /// <inheritdoc />
        public virtual IPublishedContent Parent() => _content.Parent();

        /// <inheritdoc />
        public virtual IEnumerable<IPublishedContent> Children(string culture = null) => _content.Children(culture);

        #endregion

        #region Properties

        /// <inheritdoc cref="IPublishedElement.Properties"/>
        public virtual IEnumerable<IPublishedProperty> Properties => _content.Properties;

        /// <inheritdoc cref="IPublishedElement.GetProperty(string)"/>
        public virtual IPublishedProperty GetProperty(string alias)
        {
            return _content.GetProperty(alias);
        }

        #endregion
    }
}
