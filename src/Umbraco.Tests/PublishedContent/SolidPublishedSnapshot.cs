using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.PublishedContent
{
    class SolidPublishedSnapshot : IPublishedSnapshot
    {
        public readonly SolidPublishedContentCache InnerContentCache = new SolidPublishedContentCache();
        public readonly SolidPublishedContentCache InnerMediaCache = new SolidPublishedContentCache();

        public IPublishedContentCache Content => InnerContentCache;

        public IPublishedMediaCache Media => InnerMediaCache;

        public IPublishedMemberCache Members => null;

        public IDomainCache Domains => null;

        public IDisposable ForcedPreview(bool forcedPreview, Action<bool> callback = null)
        {
            throw new NotImplementedException();
        }

        public void Resync()
        { }

        public IAppCache SnapshotCache => null;

        public IAppCache ElementsCache => null;

        public void Dispose()
        { }
    }

    class SolidPublishedContentCache : PublishedCacheBase, IPublishedContentCache2, IPublishedMediaCache2
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

        public IPublishedContent GetByRoute(bool preview, string route, bool? hideTopLevelNode = null, string culture = null)
        {
            throw new NotImplementedException();
        }

        public IPublishedContent GetByRoute(string route, bool? hideTopLevelNode = null, string culture = null)
        {
            throw new NotImplementedException();
        }

        public string GetRouteById(bool preview, int contentId, string culture = null)
        {
            throw new NotImplementedException();
        }

        public string GetRouteById(int contentId, string culture = null)
        {
            throw new NotImplementedException();
        }

        public override IPublishedContent GetById(bool preview, int contentId)
        {
            return _content.ContainsKey(contentId) ? _content[contentId] : null;
        }

        public override IPublishedContent GetById(bool preview, Guid contentId)
        {
            throw new NotImplementedException();
        }

        public override IPublishedContent GetById(bool preview, Udi nodeId)
            => throw new NotSupportedException();

        public override bool HasById(bool preview, int contentId)
        {
            return _content.ContainsKey(contentId);
        }

        public override IEnumerable<IPublishedContent> GetAtRoot(bool preview, string culture = null)
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

        public override IPublishedContentType GetContentType(int id)
        {
            throw new NotImplementedException();
        }

        public override IPublishedContentType GetContentType(string alias)
        {
            throw new NotImplementedException();
        }

        public override IPublishedContentType GetContentType(Guid key)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType)
        {
            throw new NotImplementedException();
        }
    }

    internal class SolidPublishedContent : IPublishedContent
    {
        #region Constructor

        public SolidPublishedContent(IPublishedContentType contentType)
        {
            // initialize boring stuff
            TemplateId = 0;
            WriterName = CreatorName = string.Empty;
            WriterId = CreatorId = 0;
            CreateDate = UpdateDate = DateTime.Now;
            Version = Guid.Empty;

            ContentType = contentType;
        }

        #endregion

        #region Content

        private Dictionary<string, PublishedCultureInfo> _cultures;

        private Dictionary<string, PublishedCultureInfo> GetCultures()
        {
            return new Dictionary<string, PublishedCultureInfo> { { "", new PublishedCultureInfo("", Name, UrlSegment, UpdateDate) } };
        }

        public int Id { get; set; }
        public Guid Key { get; set; }
        public int? TemplateId { get; set; }
        public int SortOrder { get; set; }
        public string Name { get; set; }
        public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures => _cultures ?? (_cultures = GetCultures());
        public string UrlSegment { get; set; }
        public string WriterName { get; set; }
        public string CreatorName { get; set; }
        public int WriterId { get; set; }
        public int CreatorId { get; set; }
        public string Path { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public Guid Version { get; set; }
        public int Level { get; set; }
        [Obsolete("Use the Url() extension instead")]
        public string Url { get; set; }

        public PublishedItemType ItemType => PublishedItemType.Content;
        public bool IsDraft(string culture = null) => false;
        public bool IsPublished(string culture = null) => true;

        #endregion

        #region Tree

        public int ParentId { get; set; }
        public IEnumerable<int> ChildIds { get; set; }

        public IPublishedContent Parent { get; set; }
        public IEnumerable<IPublishedContent> Children { get; set; }
        public IEnumerable<IPublishedContent> ChildrenForAllCultures => Children;

        #endregion

        #region ContentType

        public IPublishedContentType ContentType { get; set; }

        #endregion

        #region Properties

        public IEnumerable<IPublishedProperty> Properties { get; set; }

        public IPublishedProperty GetProperty(string alias)
        {
            return Properties.FirstOrDefault(p => p.Alias.InvariantEquals(alias));
        }

        public IPublishedProperty GetProperty(string alias, bool recurse)
        {
            var property = GetProperty(alias);
            if (recurse == false) return property;

            IPublishedContent content = this;
            while (content != null && (property == null || property.HasValue() == false))
            {
                content = content.Parent;
                property = content?.GetProperty(alias);
            }

            return property;
        }

        public object this[string alias]
        {
            get
            {
                var property = GetProperty(alias);
                return property == null || property.HasValue() == false ? null : property.GetValue();
            }
        }

        #endregion
    }

    internal class SolidPublishedProperty : IPublishedProperty
    {
        public IPublishedPropertyType PropertyType { get; set; }
        public string Alias { get; set; }
        public object SolidSourceValue { get; set; }
        public object SolidValue { get; set; }
        public bool SolidHasValue { get; set; }
        public object SolidXPathValue { get; set; }

        public virtual object GetSourceValue(string culture = null, string segment = null) => SolidSourceValue;
        public virtual object GetValue(string culture = null, string segment = null) => SolidValue;
        public virtual object GetXPathValue(string culture = null, string segment = null) => SolidXPathValue;
        public virtual bool HasValue(string culture = null, string segment = null) => SolidHasValue;
    }

    internal class SolidPublishedPropertyWithLanguageVariants : SolidPublishedProperty
    {
        private readonly IDictionary<string, object> _solidSourceValues = new Dictionary<string, object>();
        private readonly IDictionary<string, object> _solidValues = new Dictionary<string, object>();
        private readonly IDictionary<string, object> _solidXPathValues = new Dictionary<string, object>();

        public override object GetSourceValue(string culture = null, string segment = null)
        {
            if (string.IsNullOrEmpty(culture))
            {
                return base.GetSourceValue(culture, segment);
            }

            return _solidSourceValues.ContainsKey(culture) ? _solidSourceValues[culture] : null;
        }

        public override object GetValue(string culture = null, string segment = null)
        {
            if (string.IsNullOrEmpty(culture))
            {
                return base.GetValue(culture, segment);
            }

            return _solidValues.ContainsKey(culture) ? _solidValues[culture] : null;
        }

        public override object GetXPathValue(string culture = null, string segment = null)
        {
            if (string.IsNullOrEmpty(culture))
            {
                return base.GetXPathValue(culture, segment);
            }

            return _solidXPathValues.ContainsKey(culture) ? _solidXPathValues[culture] : null;
        }

        public override bool HasValue(string culture = null, string segment = null)
        {
            if (string.IsNullOrEmpty(culture))
            {
                return base.HasValue(culture, segment);
            }

            return _solidSourceValues.ContainsKey(culture);
        }

        public void SetSourceValue(string culture, object value, bool defaultValue = false)
        {
            _solidSourceValues.Add(culture, value);
            if (defaultValue)
            {
                SolidSourceValue = value;
                SolidHasValue = true;
            }
        }

        public void SetValue(string culture, object value, bool defaultValue = false)
        {
            _solidValues.Add(culture, value);
            if (defaultValue)
            {
                SolidValue = value;
                SolidHasValue = true;
            }
        }

        public void SetXPathValue(string culture, object value, bool defaultValue = false)
        {
            _solidXPathValues.Add(culture, value);
            if (defaultValue)
            {
                SolidXPathValue = value;
            }
        }
    }

    [PublishedModel("ContentType2")]
    internal class ContentType2 : PublishedContentModel
    {
        #region Plumbing

        public ContentType2(IPublishedContent content)
            : base(content)
        { }

        #endregion

        public int Prop1 => this.Value<int>("prop1");
    }

    [PublishedModel("ContentType2Sub")]
    internal class ContentType2Sub : ContentType2
    {
        #region Plumbing

        public ContentType2Sub(IPublishedContent content)
            : base(content)
        { }

        #endregion
    }

    internal class PublishedContentStrong1 : PublishedContentModel
    {
        public PublishedContentStrong1(IPublishedContent content)
            : base(content)
        { }

        public int StrongValue => this.Value<int>("strongValue");
    }

    internal class PublishedContentStrong1Sub : PublishedContentStrong1
    {
        public PublishedContentStrong1Sub(IPublishedContent content)
            : base(content)
        { }

        public int AnotherValue => this.Value<int>("anotherValue");
    }

    internal class PublishedContentStrong2 : PublishedContentModel
    {
        public PublishedContentStrong2(IPublishedContent content)
            : base(content)
        { }

        public int StrongValue => this.Value<int>("strongValue");
    }

    internal class AutoPublishedContentType : PublishedContentType
    {
        private static readonly IPublishedPropertyType Default;

        static AutoPublishedContentType()
        {
            var dataTypeService = new TestObjects.TestDataTypeService(
                new DataType(new VoidEditor(Mock.Of<ILogger>())) { Id = 666 });

            var factory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), new PropertyValueConverterCollection(Array.Empty<IPropertyValueConverter>()), dataTypeService);
            Default = factory.CreatePropertyType("*", 666);
        }

        public AutoPublishedContentType(Guid key, int id, string alias, IEnumerable<PublishedPropertyType> propertyTypes)
            : base(key, id, alias, PublishedItemType.Content, Enumerable.Empty<string>(), propertyTypes, ContentVariation.Nothing)
        { }

        public AutoPublishedContentType(Guid key, int id, string alias, Func<IPublishedContentType, IEnumerable<IPublishedPropertyType>> propertyTypes)
            : base(key, id, alias, PublishedItemType.Content, Enumerable.Empty<string>(), propertyTypes, ContentVariation.Nothing)
        { }

        public AutoPublishedContentType(Guid key, int id, string alias, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes)
            : base(key, id, alias, PublishedItemType.Content, compositionAliases, propertyTypes, ContentVariation.Nothing)
        { }

        public AutoPublishedContentType(Guid key, int id, string alias, IEnumerable<string> compositionAliases, Func<IPublishedContentType, IEnumerable<IPublishedPropertyType>> propertyTypes)
            : base(key, id, alias, PublishedItemType.Content, compositionAliases, propertyTypes, ContentVariation.Nothing)
        { }

        public override IPublishedPropertyType GetPropertyType(string alias)
        {
            var propertyType = base.GetPropertyType(alias);
            return propertyType ?? Default;
        }
    }
}
