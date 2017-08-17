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

        #region ContentSet

        public virtual IEnumerable<IPublishedContent> ContentSet
        {
            get { return Content.ContentSet; }
        }

        #endregion

        #region ContentType

        public virtual PublishedContentType ContentType { get { return Content.ContentType; } }

        #endregion

        #region Content

        public virtual int Id
        {
            get { return Content.Id; }
        }

        public virtual int TemplateId
        {
            get { return Content.TemplateId; }
        }

        public virtual int SortOrder
        {
            get { return Content.SortOrder; }
        }

        public virtual string Name
        {
            get { return Content.Name; }
        }

        public virtual string UrlName
        {
            get { return Content.UrlName; }
        }

        public virtual string DocumentTypeAlias
        {
            get { return Content.DocumentTypeAlias; }
        }

        public virtual int DocumentTypeId
        {
            get { return Content.DocumentTypeId; }
        }

        public virtual string WriterName
        {
            get { return Content.WriterName; }
        }

        public virtual string CreatorName
        {
            get { return Content.CreatorName; }
        }

        public virtual int WriterId
        {
            get { return Content.WriterId; }
        }

        public virtual int CreatorId
        {
            get { return Content.CreatorId; }
        }

        public virtual string Path
        {
            get { return Content.Path; }
        }

        public virtual DateTime CreateDate
        {
            get { return Content.CreateDate; }
        }

        public virtual DateTime UpdateDate
        {
            get { return Content.UpdateDate; }
        }

        public virtual Guid Version
        {
            get { return Content.Version; }
        }

        public virtual int Level
        {
            get { return Content.Level; }
        }

        public virtual string Url
        {
            get { return Content.Url; }
        }

        public virtual PublishedItemType ItemType
        {
            get { return Content.ItemType; }
        }

        public virtual bool IsDraft
        {
            get { return Content.IsDraft; }
        }

        public virtual int GetIndex()
        {
            return Content.GetIndex();
        }

        #endregion

        #region Tree

        public virtual IPublishedContent Parent
        {
            get { return Content.Parent; }
        }

        public virtual IEnumerable<IPublishedContent> Children
        {
            get { return Content.Children; }
        }

        #endregion

        #region Properties

        public virtual ICollection<IPublishedProperty> Properties
        {
            get { return Content.Properties; }
        }

        public virtual object this[string alias]
        {
            get { return Content[alias]; }
        }

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
