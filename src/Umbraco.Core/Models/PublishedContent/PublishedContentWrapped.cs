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

        public virtual PublishedContentType ContentType => _content.ContentType;

        #endregion

        #region Content

        public virtual int Id => _content.Id;

        public Guid Key => _content.Key;

        public virtual int TemplateId => _content.TemplateId;

        public virtual int SortOrder => _content.SortOrder;

        public virtual string Name => _content.Name;

        public virtual string UrlName => _content.UrlName;

        public virtual string DocumentTypeAlias => _content.DocumentTypeAlias;

        public virtual int DocumentTypeId => _content.DocumentTypeId;

        public virtual string WriterName => _content.WriterName;

        public virtual string CreatorName => _content.CreatorName;

        public virtual int WriterId => _content.WriterId;

        public virtual int CreatorId => _content.CreatorId;

        public virtual string Path => _content.Path;

        public virtual DateTime CreateDate => _content.CreateDate;

        public virtual DateTime UpdateDate => _content.UpdateDate;

        public virtual int Level => _content.Level;

        public virtual string Url => _content.Url;

        public virtual PublishedItemType ItemType => _content.ItemType;

        public virtual bool IsDraft => _content.IsDraft;

        #endregion

        #region Tree

        public virtual IPublishedContent Parent => _content.Parent;

        public virtual IEnumerable<IPublishedContent> Children => _content.Children;

        #endregion

        #region Properties

        public virtual IEnumerable<IPublishedProperty> Properties => _content.Properties;

        public virtual IPublishedProperty GetProperty(string alias)
        {
            return _content.GetProperty(alias);
        }

        public virtual IPublishedProperty GetProperty(string alias, bool recurse)
        {
            return _content.GetProperty(alias, recurse);
        }

        #endregion
    }
}
