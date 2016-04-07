using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.PublishedContent
{
    class SolidPublishedCaches : IPublishedCaches
    {
        public readonly SolidPublishedContentCache ContentCache = new SolidPublishedContentCache();
        public readonly SolidPublishedMediaCache MediaCache = new SolidPublishedMediaCache();

        public ContextualPublishedContentCache CreateContextualContentCache(UmbracoContext context)
        {
            return new ContextualPublishedContentCache(ContentCache, context);
        }

        public ContextualPublishedMediaCache CreateContextualMediaCache(UmbracoContext context)
        {
            return new ContextualPublishedMediaCache(MediaCache, context);
        }
    }

    class SolidPublishedContentCache : IPublishedContentCache
    {
        private readonly Dictionary<int, IPublishedContent> _content = new Dictionary<int, IPublishedContent>();

        public void Add(SolidPublishedContent content)
        {
            _content[content.Id] = content.CreateModel();
        }

        public void Clear()
        {
            _content.Clear();
        }

        public void ContentHasChanged(UmbracoContext umbracoContext)
        {
            throw new NotImplementedException();
        }

        public IPublishedContent GetByRoute(UmbracoContext umbracoContext, bool preview, string route, bool? hideTopLevelNode = null)
        {
            throw new NotImplementedException();
        }

        public string GetRouteById(UmbracoContext umbracoContext, bool preview, int contentId)
        {
            throw new NotImplementedException();
        }

        public IPublishedContent GetById(UmbracoContext umbracoContext, bool preview, int contentId)
        {
            return _content.ContainsKey(contentId) ? _content[contentId] : null;
        }

        public IEnumerable<IPublishedContent> GetAtRoot(UmbracoContext umbracoContext, bool preview)
        {
            return _content.Values.Where(x => x.Parent == null);
        }

        public IPublishedContent GetSingleByXPath(UmbracoContext umbracoContext, bool preview, string xpath, Core.Xml.XPathVariable[] vars)
        {
            throw new NotImplementedException();
        }

        public IPublishedContent GetSingleByXPath(UmbracoContext umbracoContext, bool preview, System.Xml.XPath.XPathExpression xpath, Core.Xml.XPathVariable[] vars)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPublishedContent> GetByXPath(UmbracoContext umbracoContext, bool preview, string xpath, Core.Xml.XPathVariable[] vars)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPublishedContent> GetByXPath(UmbracoContext umbracoContext, bool preview, System.Xml.XPath.XPathExpression xpath, Core.Xml.XPathVariable[] vars)
        {
            throw new NotImplementedException();
        }

        public System.Xml.XPath.XPathNavigator GetXPathNavigator(UmbracoContext umbracoContext, bool preview)
        {
            throw new NotImplementedException();
        }

        public bool XPathNavigatorIsNavigable
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasContent(UmbracoContext umbracoContext, bool preview)
        {
            return _content.Count > 0;
        }

        public IPublishedProperty CreateDetachedProperty(PublishedPropertyType propertyType, object value, bool isPreviewing)
        {
            throw new NotImplementedException();
        }
    }

    class SolidPublishedMediaCache : IPublishedMediaCache
    {
        private readonly Dictionary<int, IPublishedContent> _media = new Dictionary<int, IPublishedContent>();

        public void Add(SolidPublishedContent content)
        {
            _media[content.Id] = content.CreateModel();
        }

        public void Clear()
        {
            _media.Clear();
        }

        public IPublishedContent GetById(UmbracoContext umbracoContext, bool preview, int contentId)
        {
            return _media.ContainsKey(contentId) ? _media[contentId] : null;
        }

        public IEnumerable<IPublishedContent> GetAtRoot(UmbracoContext umbracoContext, bool preview)
        {
            return _media.Values.Where(x => x.Parent == null);
        }

        public IPublishedContent GetSingleByXPath(UmbracoContext umbracoContext, bool preview, string xpath, Core.Xml.XPathVariable[] vars)
        {
            throw new NotImplementedException();
        }

        public IPublishedContent GetSingleByXPath(UmbracoContext umbracoContext, bool preview, System.Xml.XPath.XPathExpression xpath, Core.Xml.XPathVariable[] vars)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPublishedContent> GetByXPath(UmbracoContext umbracoContext, bool preview, string xpath, Core.Xml.XPathVariable[] vars)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPublishedContent> GetByXPath(UmbracoContext umbracoContext, bool preview, System.Xml.XPath.XPathExpression xpath, Core.Xml.XPathVariable[] vars)
        {
            throw new NotImplementedException();
        }

        public System.Xml.XPath.XPathNavigator GetXPathNavigator(UmbracoContext umbracoContext, bool preview)
        {
            throw new NotImplementedException();
        }

        public bool XPathNavigatorIsNavigable
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasContent(UmbracoContext umbracoContext, bool preview)
        {
            return _media.Count > 0;
        }
    }

    class SolidPublishedContent : IPublishedContent
    {
        #region Constructor

        public SolidPublishedContent(PublishedContentType contentType)
        {
            // initialize boring stuff
            TemplateId = 0;
            WriterName = CreatorName = string.Empty;
            WriterId = CreatorId = 0;
            CreateDate = UpdateDate = DateTime.Now;
            Version = Guid.Empty;
            IsDraft = false;

            ContentType = contentType;
            DocumentTypeAlias = contentType.Alias;
            DocumentTypeId = contentType.Id;
        }

        #endregion

        #region Content

        public int Id { get; set; }
        public int TemplateId { get; set; }
        public int SortOrder { get; set; }
        public string Name { get; set; }
        public string UrlName { get; set; }
        public string DocumentTypeAlias { get; private set; }
        public int DocumentTypeId { get; private set; }
        public string WriterName { get; set; }
        public string CreatorName { get; set; }
        public int WriterId { get; set; }
        public int CreatorId { get; set; }
        public string Path { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public Guid Version { get; set; }
        public int Level { get; set; }
        public string Url { get; set; }

        public PublishedItemType ItemType { get { return PublishedItemType.Content; } }
        public bool IsDraft { get; set; }

        public int GetIndex()
        {
            var index = this.Siblings().FindIndex(x => x.Id == Id);
            if (index < 0)
                throw new IndexOutOfRangeException("Failed to find content in its siblings collection?!");
            return index;
        }

        #endregion

        #region Tree

        public int ParentId { get; set; }
        public IEnumerable<int> ChildIds { get; set; }

        public IPublishedContent Parent { get { return UmbracoContext.Current.ContentCache.GetById(ParentId); } }
        public IEnumerable<IPublishedContent> Children { get { return ChildIds.Select(id => UmbracoContext.Current.ContentCache.GetById(id)); } }

        #endregion

        #region ContentSet

        public IEnumerable<IPublishedContent> ContentSet { get { return this.Siblings(); } }

        #endregion

        #region ContentType

        public PublishedContentType ContentType { get; private set; }

        #endregion

        #region Properties

        public ICollection<IPublishedProperty> Properties { get; set; }

        public IPublishedProperty GetProperty(string alias)
        {
            return Properties.FirstOrDefault(p => p.PropertyTypeAlias.InvariantEquals(alias));
        }

        public IPublishedProperty GetProperty(string alias, bool recurse)
        {
            var property = GetProperty(alias);
            if (recurse == false) return property;

            IPublishedContent content = this;
            while (content != null && (property == null || property.HasValue == false))
            {
                content = content.Parent;
                property = content == null ? null : content.GetProperty(alias);
            }

            return property;
        }

        public object this[string alias]
        {
            get
            {
                var property = GetProperty(alias);
                return property == null || property.HasValue == false ? null : property.Value;
            }
        }

        #endregion
    }

    class SolidPublishedProperty : IPublishedProperty
    {
        public SolidPublishedProperty()
        {
            // initialize boring stuff
        }

        public string PropertyTypeAlias { get; set; }
        public object DataValue { get; set; }
        public object Value { get; set; }
        public bool HasValue { get; set; }
        public object XPathValue { get; set; }
    }

    [PublishedContentModel("ContentType2")]
    internal class ContentType2 : PublishedContentModel
    {
        #region Plumbing

        public ContentType2(IPublishedContent content)
            : base(content)
        { }

        #endregion

        // fast, if you know that the appropriate IPropertyEditorValueConverter is wired
        public int Prop1 { get { return (int)this["prop1"]; } }

        // almost as fast, not sure I like it as much, though
        //public int Prop1 { get { return this.GetPropertyValue<int>("prop1"); } }
    }

    [PublishedContentModel("ContentType2Sub")]
    internal class ContentType2Sub : ContentType2
    {
        #region Plumbing

        public ContentType2Sub(IPublishedContent content)
            : base(content)
        { }

        #endregion
    }

    class PublishedContentStrong1 : PublishedContentExtended
    {
        public PublishedContentStrong1(IPublishedContent content)
            : base(content)
        { }

        public int StrongValue { get { return (int)this["strongValue"]; } }
    }

    class PublishedContentStrong1Sub : PublishedContentStrong1
    {
        public PublishedContentStrong1Sub(IPublishedContent content)
            : base(content)
        { }

        public int AnotherValue { get { return (int)this["anotherValue"]; } }
    }

    class PublishedContentStrong2 : PublishedContentExtended
    {
        public PublishedContentStrong2(IPublishedContent content)
            : base(content)
        { }

        public int StrongValue { get { return (int)this["strongValue"]; } }
    }

    class AutoPublishedContentType : PublishedContentType
    {
        private static readonly PublishedPropertyType Default = new PublishedPropertyType("*", 0, "?");

        public AutoPublishedContentType(int id, string alias, IEnumerable<PublishedPropertyType> propertyTypes)
            : base(id, alias, Enumerable.Empty<string>(), propertyTypes)
        { }

        public AutoPublishedContentType(int id, string alias, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes)
            : base(id, alias, compositionAliases, propertyTypes)
        { }

        public override PublishedPropertyType GetPropertyType(string alias)
        {
            var propertyType = base.GetPropertyType(alias);
            return propertyType ?? Default;
        }
    }
}
