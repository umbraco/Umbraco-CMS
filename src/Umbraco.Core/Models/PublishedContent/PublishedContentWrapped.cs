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
        protected readonly IPublishedContent WrappedContentInternal;

        /// <summary>
        /// Initialize a new instance of the <see cref="PublishedContentWrapped"/> class
        /// with an <c>IPublishedContent</c> instance to wrap and extend.
        /// </summary>
        /// <param name="content">The content to wrap and extend.</param>
        protected PublishedContentWrapped(IPublishedContent content)
        {
            WrappedContentInternal = content;
        }

        /// <summary>
        /// Gets the wrapped content.
        /// </summary>
        /// <returns>The wrapped content, that was passed as an argument to the constructor.</returns>
        public IPublishedContent Unwrap()
        {
            return WrappedContentInternal;
        }

        #region ContentSet

        public virtual IEnumerable<IPublishedContent> ContentSet
        {
            get { return WrappedContentInternal.ContentSet; }
        }

        #endregion

        #region ContentType

        public virtual PublishedContentType ContentType { get { return WrappedContentInternal.ContentType; } }

        #endregion

        #region Content

        public virtual int Id
        {
            get { return WrappedContentInternal.Id; }
        }

        public virtual int TemplateId
        {
            get { return WrappedContentInternal.TemplateId; }
        }

        public virtual int SortOrder
        {
            get { return WrappedContentInternal.SortOrder; }
        }

        public virtual string Name
        {
            get { return WrappedContentInternal.Name; }
        }

        public virtual string UrlName
        {
            get { return WrappedContentInternal.UrlName; }
        }

        public virtual string DocumentTypeAlias
        {
            get { return WrappedContentInternal.DocumentTypeAlias; }
        }

        public virtual int DocumentTypeId
        {
            get { return WrappedContentInternal.DocumentTypeId; }
        }

        public virtual string WriterName
        {
            get { return WrappedContentInternal.WriterName; }
        }

        public virtual string CreatorName
        {
            get { return WrappedContentInternal.CreatorName; }
        }

        public virtual int WriterId
        {
            get { return WrappedContentInternal.WriterId; }
        }

        public virtual int CreatorId
        {
            get { return WrappedContentInternal.CreatorId; }
        }

        public virtual string Path
        {
            get { return WrappedContentInternal.Path; }
        }

        public virtual DateTime CreateDate
        {
            get { return WrappedContentInternal.CreateDate; }
        }

        public virtual DateTime UpdateDate
        {
            get { return WrappedContentInternal.UpdateDate; }
        }

        public virtual Guid Version
        {
            get { return WrappedContentInternal.Version; }
        }

        public virtual int Level
        {
            get { return WrappedContentInternal.Level; }
        }

        public virtual string Url
        {
            get { return WrappedContentInternal.Url; }
        }

        public virtual PublishedItemType ItemType
        {
            get { return WrappedContentInternal.ItemType; }
        }

        public virtual bool IsDraft
        {
            get { return WrappedContentInternal.IsDraft; }
        }

        public virtual int GetIndex()
        {
            return WrappedContentInternal.GetIndex();
        }

        #endregion

        #region Tree

        public virtual IPublishedContent Parent
        {
            get { return WrappedContentInternal.Parent; }
        }

        public virtual IEnumerable<IPublishedContent> Children
        {
            get { return WrappedContentInternal.Children; }
        }

        #endregion

        #region Properties

        public virtual ICollection<IPublishedProperty> Properties
        {
            get { return WrappedContentInternal.Properties; }
        }

        public virtual object this[string alias]
        {
            get { return WrappedContentInternal[alias]; }
        }

        public virtual IPublishedProperty GetProperty(string alias)
        {
            return WrappedContentInternal.GetProperty(alias);
        }

        public virtual IPublishedProperty GetProperty(string alias, bool recurse)
        {
            return WrappedContentInternal.GetProperty(alias, recurse);
        }

        #endregion
    }
}
