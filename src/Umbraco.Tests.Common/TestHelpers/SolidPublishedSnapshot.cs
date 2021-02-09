// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Xml;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Common.PublishedContent
{
    public class SolidPublishedSnapshot : IPublishedSnapshot
    {
        public readonly SolidPublishedContentCache InnerContentCache = new SolidPublishedContentCache();
        public readonly SolidPublishedContentCache InnerMediaCache = new SolidPublishedContentCache();

        public IPublishedContentCache Content => InnerContentCache;

        public IPublishedMediaCache Media => InnerMediaCache;

        public IPublishedMemberCache Members => null;

        public IDomainCache Domains => null;

        public IDisposable ForcedPreview(bool forcedPreview, Action<bool> callback = null) => throw new NotImplementedException();

        public void Resync()
        {
        }

        public IAppCache SnapshotCache => null;

        public IAppCache ElementsCache => null;

        public void Dispose()
        {
        }
    }

    public class SolidPublishedContentCache : PublishedCacheBase, IPublishedContentCache, IPublishedMediaCache
    {
        private readonly Dictionary<int, IPublishedContent> _content = new Dictionary<int, IPublishedContent>();

        public SolidPublishedContentCache()
            : base(false)
        {
        }

        public void Add(SolidPublishedContent content) => _content[content.Id] = content.CreateModel(Mock.Of<IPublishedModelFactory>());

        public void Clear() => _content.Clear();

        public IPublishedContent GetByRoute(bool preview, string route, bool? hideTopLevelNode = null, string culture = null) => throw new NotImplementedException();

        public IPublishedContent GetByRoute(string route, bool? hideTopLevelNode = null, string culture = null) => throw new NotImplementedException();

        public string GetRouteById(bool preview, int contentId, string culture = null) => throw new NotImplementedException();

        public string GetRouteById(int contentId, string culture = null) => throw new NotImplementedException();

        public override IPublishedContent GetById(bool preview, int contentId) => _content.ContainsKey(contentId) ? _content[contentId] : null;

        public override IPublishedContent GetById(bool preview, Guid contentId) => throw new NotImplementedException();

        public override IPublishedContent GetById(bool preview, Udi nodeId) => throw new NotSupportedException();

        public override bool HasById(bool preview, int contentId) => _content.ContainsKey(contentId);

        public override IEnumerable<IPublishedContent> GetAtRoot(bool preview, string culture = null) => _content.Values.Where(x => x.Parent == null);

        public override IPublishedContent GetSingleByXPath(bool preview, string xpath, XPathVariable[] vars) => throw new NotImplementedException();

        public override IPublishedContent GetSingleByXPath(bool preview, System.Xml.XPath.XPathExpression xpath, XPathVariable[] vars) => throw new NotImplementedException();

        public override IEnumerable<IPublishedContent> GetByXPath(bool preview, string xpath, XPathVariable[] vars) => throw new NotImplementedException();

        public override IEnumerable<IPublishedContent> GetByXPath(bool preview, System.Xml.XPath.XPathExpression xpath, XPathVariable[] vars) => throw new NotImplementedException();

        public override System.Xml.XPath.XPathNavigator CreateNavigator(bool preview) => throw new NotImplementedException();

        public override System.Xml.XPath.XPathNavigator CreateNodeNavigator(int id, bool preview) => throw new NotImplementedException();

        public override bool HasContent(bool preview) => _content.Count > 0;

        public override IPublishedContentType GetContentType(int id) => throw new NotImplementedException();

        public override IPublishedContentType GetContentType(string alias) => throw new NotImplementedException();

        public override IPublishedContentType GetContentType(Guid key) => throw new NotImplementedException();

        public override IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType) => throw new NotImplementedException();
    }

    public class SolidPublishedContent : IPublishedContent
    {
        public SolidPublishedContent(IPublishedContentType contentType)
        {
            // initialize boring stuff
            TemplateId = 0;
            WriterId = CreatorId = 0;
            CreateDate = UpdateDate = DateTime.Now;
            Version = Guid.Empty;

            ContentType = contentType;
        }

        private Dictionary<string, PublishedCultureInfo> _cultures;

        private Dictionary<string, PublishedCultureInfo> GetCultures() => new Dictionary<string, PublishedCultureInfo> { { string.Empty, new PublishedCultureInfo(string.Empty, Name, UrlSegment, UpdateDate) } };

        public int Id { get; set; }

        public Guid Key { get; set; }

        public int? TemplateId { get; set; }

        public int SortOrder { get; set; }

        public string Name { get; set; }

        public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures => _cultures ?? (_cultures = GetCultures());

        public string UrlSegment { get; set; }

        public int WriterId { get; set; }

        public int CreatorId { get; set; }

        public string Path { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public Guid Version { get; set; }

        public int Level { get; set; }

        public PublishedItemType ItemType => PublishedItemType.Content;

        public bool IsDraft(string culture = null) => false;

        public bool IsPublished(string culture = null) => true;

        public int ParentId { get; set; }

        public IEnumerable<int> ChildIds { get; set; }

        public IPublishedContent Parent { get; set; }

        public IEnumerable<IPublishedContent> Children { get; set; }

        public IEnumerable<IPublishedContent> ChildrenForAllCultures => Children;

        public IPublishedContentType ContentType { get; set; }

        public IEnumerable<IPublishedProperty> Properties { get; set; }

        public IPublishedProperty GetProperty(string alias) => Properties.FirstOrDefault(p => p.Alias.InvariantEquals(alias));

        public IPublishedProperty GetProperty(string alias, bool recurse)
        {
            IPublishedProperty property = GetProperty(alias);
            if (recurse == false)
            {
                return property;
            }

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
    }

    public class SolidPublishedProperty : IPublishedProperty
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

    public class SolidPublishedPropertyWithLanguageVariants : SolidPublishedProperty
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
    public class ContentType2 : PublishedContentModel
    {
        public ContentType2(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content)
        {
        }

        public int Prop1 => this.Value<int>(Mock.Of<IPublishedValueFallback>(), "prop1");
    }

    [PublishedModel("ContentType2Sub")]
    public class ContentType2Sub : ContentType2
    {
        public ContentType2Sub(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }
    }

    public class PublishedContentStrong1 : PublishedContentModel
    {
        public PublishedContentStrong1(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content)
        {
        }

        public int StrongValue => this.Value<int>(Mock.Of<IPublishedValueFallback>(), "strongValue");
    }

    public class PublishedContentStrong1Sub : PublishedContentStrong1
    {
        public PublishedContentStrong1Sub(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }

        public int AnotherValue => this.Value<int>(Mock.Of<IPublishedValueFallback>(), "anotherValue");
    }

    public class PublishedContentStrong2 : PublishedContentModel
    {
        public PublishedContentStrong2(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content)
        {
        }

        public int StrongValue => this.Value<int>(Mock.Of<IPublishedValueFallback>(), "strongValue");
    }

    public class AutoPublishedContentType : PublishedContentType
    {
        private static readonly IPublishedPropertyType Default;

        static AutoPublishedContentType()
        {
            var configurationEditorJsonSerializer = new ConfigurationEditorJsonSerializer();
            var jsonSerializer = new JsonNetSerializer();
            var dataTypeServiceMock = new Mock<IDataTypeService>();

            var dataType = new DataType(
                new VoidEditor(
                    Mock.Of<ILoggerFactory>(),
                    dataTypeServiceMock.Object,
                    Mock.Of<ILocalizationService>(),
                    Mock.Of<ILocalizedTextService>(),
                    Mock.Of<IShortStringHelper>(),
                    jsonSerializer),
                configurationEditorJsonSerializer)
                {
                    Id = 666
                };
            dataTypeServiceMock.Setup(x => x.GetAll()).Returns(dataType.Yield);

            var factory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), new PropertyValueConverterCollection(Array.Empty<IPropertyValueConverter>()), dataTypeServiceMock.Object);
            Default = factory.CreatePropertyType("*", 666);
        }

        public AutoPublishedContentType(Guid key, int id, string alias, IEnumerable<PublishedPropertyType> propertyTypes)
            : base(key, id, alias, PublishedItemType.Content, Enumerable.Empty<string>(), propertyTypes, ContentVariation.Nothing)
        {
        }

        public AutoPublishedContentType(Guid key, int id, string alias, Func<IPublishedContentType, IEnumerable<IPublishedPropertyType>> propertyTypes)
            : base(key, id, alias, PublishedItemType.Content, Enumerable.Empty<string>(), propertyTypes, ContentVariation.Nothing)
        {
        }

        public AutoPublishedContentType(Guid key, int id, string alias, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes)
            : base(key, id, alias, PublishedItemType.Content, compositionAliases, propertyTypes, ContentVariation.Nothing)
        {
        }

        public AutoPublishedContentType(Guid key, int id, string alias, IEnumerable<string> compositionAliases, Func<IPublishedContentType, IEnumerable<IPublishedPropertyType>> propertyTypes)
            : base(key, id, alias, PublishedItemType.Content, compositionAliases, propertyTypes, ContentVariation.Nothing)
        {
        }

        public override IPublishedPropertyType GetPropertyType(string alias)
        {
            IPublishedPropertyType propertyType = base.GetPropertyType(alias);
            return propertyType ?? Default;
        }
    }
}
