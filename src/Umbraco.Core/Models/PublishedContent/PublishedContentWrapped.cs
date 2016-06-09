using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.PublishedContent
{
    //
    // This class has two purposes.
    //
    // - First, we cannot implement strongly-typed content by inheriting from some sort
    // of "master content" because that master content depends on the actual content cache
    // that is being used. It can be an XmlPublishedContent with the XmlPublishedCache,
    // or just anything else.
    //
    // So we implement strongly-typed content by encapsulating whatever content is
    // returned by the content cache, and providing extra properties (mostly) or
    // methods or whatever. This class provides the base for such encapsulation.
    //
    // - Second, any time a content is used in a content set obtained from
    // IEnumerable<IPublishedContent>.ToContentSet(), it needs to be cloned and extended
    // in order to know about its position in the set.  This class provides the base
    // for implementing such extension.
    //

    /// <summary>
    /// Provides an abstract base class for <c>IPublishedContent</c> implementations that
    /// wrap and extend another <c>IPublishedContent</c>.
    /// </summary>
    public abstract class PublishedContentWrapped : IPublishedContent
    {
        protected readonly IPublishedContent Content;

        /// <summary>
        /// Initialize a new instance of the <see cref="PublishedContentWrapped"/> class
        /// with an <c>IPublishedContent</c> instance to wrap and extend.
        /// </summary>
        /// <param name="content">The content to wrap and extend.</param>
        protected PublishedContentWrapped(IPublishedContent content)
        {
            Content = content;
        }

        /// <summary>
        /// Gets the wrapped content.
        /// </summary>
        /// <returns>The wrapped content, that was passed as an argument to the constructor.</returns>
        public IPublishedContent Unwrap()
        {
            return Content;
        }

        #region ContentType

        public virtual PublishedContentType ContentType => Content.ContentType;

        #endregion

        #region Content

        public virtual int Id => Content.Id;

        public Guid Key => Content.Key;

        public virtual int TemplateId => Content.TemplateId;

        public virtual int SortOrder => Content.SortOrder;

        public virtual string Name => Content.Name;

        public virtual string UrlName => Content.UrlName;

        public virtual string DocumentTypeAlias => Content.DocumentTypeAlias;

        public virtual int DocumentTypeId => Content.DocumentTypeId;

        public virtual string WriterName => Content.WriterName;

        public virtual string CreatorName => Content.CreatorName;

        public virtual int WriterId => Content.WriterId;

        public virtual int CreatorId => Content.CreatorId;

        public virtual string Path => Content.Path;

        public virtual DateTime CreateDate => Content.CreateDate;

        public virtual DateTime UpdateDate => Content.UpdateDate;

        public virtual Guid Version => Content.Version;

        public virtual int Level => Content.Level;

        public virtual string Url => Content.Url;

        public virtual PublishedItemType ItemType => Content.ItemType;

        public virtual bool IsDraft => Content.IsDraft;

        #endregion

        #region Tree

        public virtual IPublishedContent Parent => Content.Parent;

        public virtual IEnumerable<IPublishedContent> Children => Content.Children;

        #endregion

        #region Properties

        public virtual ICollection<IPublishedProperty> Properties => Content.Properties;

        public virtual object this[string alias] => Content[alias];

        public virtual IPublishedProperty GetProperty(string alias)
        {
            return Content.GetProperty(alias);
        }

        public virtual IPublishedProperty GetProperty(string alias, bool recurse)
        {
            return Content.GetProperty(alias, recurse);
        }

        #endregion
    }
}
