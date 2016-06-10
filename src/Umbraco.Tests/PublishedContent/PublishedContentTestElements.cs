using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.PublishedContent
{
    class SolidFacade : IFacade
    {
        public readonly SolidPublishedContentCache InnerContentCache = new SolidPublishedContentCache();
        public readonly SolidPublishedContentCache InnerMediaCache = new SolidPublishedContentCache();

        public IPublishedContentCache ContentCache => InnerContentCache;

        public IPublishedMediaCache MediaCache => InnerMediaCache;

        public IPublishedMemberCache MemberCache => null;

        public IDomainCache DomainCache => null;

        public IDisposable ForcedPreview(bool forcedPreview, Action<bool> callback = null)
        {
            throw new NotImplementedException();
        }

        public IPublishedProperty CreateFragmentProperty(PublishedPropertyType propertyType, Guid itemKey, bool previewing, PropertyCacheLevel referenceCacheLevel, object sourceValue = null)
        {
            throw new NotImplementedException();
        }

        public void Resync()
        { }
    }

    class SolidPublishedContentCache : PublishedCacheBase, IPublishedContentCache, IPublishedMediaCache
    {
        private readonly Dictionary<int, IPublishedContent> _content = new Dictionary<int, IPublishedContent>();

        public SolidPublishedContentCache()
            : base(false)
        { }

        public void Add(SolidPublishedContent content)
        {
            _content[content.Id] = content.CreateModel();
        }

        public void Clear()
        {
            _content.Clear();
        }

        public IPublishedContent GetByRoute(bool preview, string route, bool? hideTopLevelNode = null)
        {
            throw new NotImplementedException();
        }

        public IPublishedContent GetByRoute(string route, bool? hideTopLevelNode = null)
        {
            throw new NotImplementedException();
        }

        public string GetRouteById(bool preview, int contentId)
        {
            throw new NotImplementedException();
        }

        public string GetRouteById(int contentId)
        {
            throw new NotImplementedException();
        }

        public override IPublishedContent GetById(bool preview, int contentId)
        {
            return _content.ContainsKey(contentId) ? _content[contentId] : null;
        }

        public override bool HasById(bool preview, int contentId)
        {
            return _content.ContainsKey(contentId);
        }

        public override IEnumerable<IPublishedContent> GetAtRoot(bool preview)
        {
            return _content.Values.Where(x => x.Parent == null);
        }

        public override IPublishedContent GetSingleByXPath(bool preview, string xpath, Core.Xml.XPathVariable[] vars)
        {
            throw new NotImplementedException();
        }

        public override IPublishedContent GetSingleByXPath(bool preview, System.Xml.XPath.XPathExpression xpath, Core.Xml.XPathVariable[] vars)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IPublishedContent> GetByXPath(bool preview, string xpath, Core.Xml.XPathVariable[] vars)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IPublishedContent> GetByXPath(bool preview, System.Xml.XPath.XPathExpression xpath, Core.Xml.XPathVariable[] vars)
        {
            throw new NotImplementedException();
        }

        public override System.Xml.XPath.XPathNavigator CreateNavigator(bool preview)
        {
            throw new NotImplementedException();
        }

        public override System.Xml.XPath.XPathNavigator CreateNodeNavigator(int id, bool preview)
        {
            throw new NotImplementedException();
        }

        public override bool HasContent(bool preview)
        {
            return _content.Count > 0;
        }

        public override PublishedContentType GetContentType(int id)
        {
            throw new NotImplementedException();
        }

        public override PublishedContentType GetContentType(string alias)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IPublishedContent> GetByContentType(PublishedContentType contentType)
        {
            throw new NotImplementedException();
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
        public Guid Key { get; set; }
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

        #endregion

        #region Tree

        public int ParentId { get; set; }
        public IEnumerable<int> ChildIds { get; set; }

        public IPublishedContent Parent { get; set; }
        public IEnumerable<IPublishedContent> Children { get; set; }

        #endregion

        #region ContentType

        public PublishedContentType ContentType { get; private set; }

        #endregion

        #region Properties

        public IEnumerable<IPublishedProperty> Properties { get; set; }

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
        public object SourceValue { get; set; }
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

        public int Prop1 => this.GetPropertyValue<int>("prop1");
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

        public int StrongValue => this.GetPropertyValue<int>("strongValue");
    }

    class PublishedContentStrong1Sub : PublishedContentStrong1
    {
        public PublishedContentStrong1Sub(IPublishedContent content)
            : base(content)
        { }

        public int AnotherValue => this.GetPropertyValue<int>("anotherValue");
    }

    class PublishedContentStrong2 : PublishedContentExtended
    {
        public PublishedContentStrong2(IPublishedContent content)
            : base(content)
        { }

        public int StrongValue => this.GetPropertyValue<int>("strongValue");
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
