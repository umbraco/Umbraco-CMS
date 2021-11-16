using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;
using Umbraco.Web.Composing;
using Umbraco.Web.Editors;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Macros
{
    /// <summary>
    /// Legacy class used by macros which converts a published content item into a hashset of values
    /// </summary>
    internal class PublishedContentHashtableConverter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedContentHashtableConverter"/> class for a published document request.
        /// </summary>
        /// <param name="frequest">The <see cref="PublishedRequest"/> pointing to the document.</param>
        /// <remarks>
        /// The difference between creating the page with PublishedRequest vs an IPublishedContent item is
        /// that the PublishedRequest takes into account how a template is assigned during the routing process whereas
        /// with an IPublishedContent item, the template id is assigned purely based on the default.
        /// </remarks>
        internal PublishedContentHashtableConverter(PublishedRequest frequest)
        {
            if (!frequest.HasPublishedContent)
                throw new ArgumentException("Document request has no node.", nameof(frequest));

            PopulatePageData(frequest.PublishedContent.Id,
                frequest.PublishedContent.Name, frequest.PublishedContent.ContentType.Id, frequest.PublishedContent.ContentType.Alias,
                frequest.PublishedContent.WriterName, frequest.PublishedContent.CreatorName, frequest.PublishedContent.CreateDate, frequest.PublishedContent.UpdateDate,
                frequest.PublishedContent.Path, frequest.PublishedContent.Parent?.Id ?? -1);

            if (frequest.HasTemplate)
            {
                Elements["template"] = frequest.TemplateModel.Id.ToString();
            }

            PopulateElementData(frequest.PublishedContent);

        }

        /// <summary>
        /// Initializes a new instance of the page for a published document
        /// </summary>
        /// <param name="doc"></param>
        internal PublishedContentHashtableConverter(IPublishedContent doc)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));

            PopulatePageData(doc.Id,
                doc.Name, doc.ContentType.Id, doc.ContentType.Alias,
                doc.WriterName, doc.CreatorName, doc.CreateDate, doc.UpdateDate,
                doc.Path, doc.Parent?.Id ?? -1);

            if (doc.TemplateId.HasValue)
            {
                //set the template to whatever is assigned to the doc
                Elements["template"] = doc.TemplateId.Value.ToString();
            }

            PopulateElementData(doc);
        }

        /// <summary>
        /// Initializes a new instance of the page for a content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="variationContextAccessor"></param>
        /// <remarks>This is for <see cref="MacroRenderingController"/> usage only.</remarks>
        internal PublishedContentHashtableConverter(IContent content, IVariationContextAccessor variationContextAccessor)
            : this(new PagePublishedContent(content, variationContextAccessor))
        { }

        #endregion

        #region Initialize

        private void PopulatePageData(int pageId,
            string pageName, int nodeType, string nodeTypeAlias,
            string writerName, string creatorName, DateTime createDate, DateTime updateDate,
            string path, int parentId)
        {
            // Update the elements hashtable
            Elements.Add("pageID", pageId);
            Elements.Add("parentID", parentId);
            Elements.Add("pageName", pageName);
            Elements.Add("nodeType", nodeType);
            Elements.Add("nodeTypeAlias", nodeTypeAlias);
            Elements.Add("writerName", writerName);
            Elements.Add("creatorName", creatorName);
            Elements.Add("createDate", createDate);
            Elements.Add("updateDate", updateDate);
            Elements.Add("path", path);
            Elements.Add("splitpath", path.Split(Constants.CharArrays.Comma));
        }

        /// <summary>
        /// Puts the properties of the node into the elements table
        /// </summary>
        /// <param name="node"></param>
        private void PopulateElementData(IPublishedElement node)
        {
            foreach (var p in node.Properties)
            {
                if (Elements.ContainsKey(p.Alias) == false)
                {
                    // note: legacy used the raw value (see populating from an Xml node below)
                    // so we're doing the same here, using DataValue. If we use Value then every
                    // value will be converted NOW - including RTEs that may contain macros that
                    // require that the 'page' is already initialized = catch-22.

                    // to properly fix this, we'd need to turn the elements collection into some
                    // sort of collection of lazy values.

                    Elements[p.Alias] = p.GetSourceValue();
                }
            }
        }
        #endregion

        /// <summary>
        /// Returns a Hashtable of data for a published content item
        /// </summary>
        public Hashtable Elements { get; } = new Hashtable();


        #region PublishedContent

        private class PagePublishedProperty : PublishedPropertyBase
        {
            private readonly object _sourceValue;
            private readonly IPublishedContent _content;

            public PagePublishedProperty(IPublishedPropertyType propertyType, IPublishedContent content)
                : base(propertyType, PropertyCacheLevel.Unknown) // cache level is ignored
            {
                _sourceValue = null;
                _content = content;
            }

            public PagePublishedProperty(IPublishedPropertyType propertyType, IPublishedContent content, Umbraco.Core.Models.Property property)
                : base(propertyType, PropertyCacheLevel.Unknown) // cache level is ignored
            {
                _sourceValue = property.GetValue();
                _content = content;
            }

            public override bool HasValue(string culture = null, string segment = null)
            {
                return _sourceValue != null && ((_sourceValue is string) == false || string.IsNullOrWhiteSpace((string)_sourceValue) == false);
            }

            public override object GetSourceValue(string culture = null, string segment = null)
            {
                return _sourceValue;
            }

            public override object GetValue(string culture = null, string segment = null)
            {
                // isPreviewing is true here since we want to preview anyway...
                const bool isPreviewing = true;
                var source = PropertyType.ConvertSourceToInter(_content, _sourceValue, isPreviewing);
                return PropertyType.ConvertInterToObject(_content, PropertyCacheLevel.Unknown, source, isPreviewing);
            }

            public override object GetXPathValue(string culture = null, string segment = null)
            {
                throw new NotImplementedException();
            }
        }

        private class PagePublishedContent : IPublishedContent
        {
            private readonly IContent _inner;
            private readonly IPublishedProperty[] _properties;
            private IReadOnlyDictionary<string, PublishedCultureInfo> _cultureInfos;
            private readonly IVariationContextAccessor _variationContextAccessor;

            private static readonly IReadOnlyDictionary<string, PublishedCultureInfo> NoCultureInfos = new Dictionary<string, PublishedCultureInfo>();

            private PagePublishedContent(int id)
            {
                Id = id;
            }

            public PagePublishedContent(IContent inner, IVariationContextAccessor variationContextAccessor)
            {
                _inner = inner ?? throw new NullReferenceException("content");
                _variationContextAccessor = variationContextAccessor;
                Id = _inner.Id;
                Key = _inner.Key;

                CreatorName = _inner.GetCreatorProfile()?.Name;
                WriterName = _inner.GetWriterProfile()?.Name;

                // TODO: inject
                var contentType = Current.Services.ContentTypeBaseServices.GetContentTypeOf(_inner);
                ContentType = Current.PublishedContentTypeFactory.CreateContentType(contentType);

                _properties = ContentType.PropertyTypes
                    .Select(x =>
                    {
                        var p = _inner.Properties.SingleOrDefault(xx => xx.Alias == x.Alias);
                        return p == null ? new PagePublishedProperty(x, this) : new PagePublishedProperty(x, this, p);
                    })
                    .Cast<IPublishedProperty>()
                    .ToArray();

                Parent = new PagePublishedContent(_inner.ParentId);
            }

            public IPublishedContentType ContentType { get; }

            public int Id { get; }

            public Guid Key { get; }

            public int? TemplateId => _inner.TemplateId;

            public int SortOrder => _inner.SortOrder;

            public string Name => _inner.Name;

            public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures
            {
                get
                {
                    if (!_inner.ContentType.VariesByCulture())
                        return NoCultureInfos;

                    if (_cultureInfos != null)
                        return _cultureInfos;

                    var urlSegmentProviders = Current.UrlSegmentProviders; // TODO inject
                    return _cultureInfos = _inner.PublishCultureInfos.Values
                        .ToDictionary(x => x.Culture, x => new PublishedCultureInfo(x.Culture, x.Name, _inner.GetUrlSegment(urlSegmentProviders, x.Culture), x.Date));
                }
            }

            public string UrlSegment => throw new NotImplementedException();

            [Obsolete("Use WriterName(IUserService) extension instead")]
            public string WriterName { get; }

            [Obsolete("Use CreatorName(IUserService) extension instead")]
            public string CreatorName { get; }

            public int WriterId => _inner.WriterId;

            public int CreatorId => _inner.CreatorId;

            public string Path => _inner.Path;

            public DateTime CreateDate => _inner.CreateDate;

            public DateTime UpdateDate => _inner.UpdateDate;

            public int Level => _inner.Level;

            public string Url => throw new NotImplementedException();

            public PublishedItemType ItemType => PublishedItemType.Content;

            public bool IsDraft(string culture = null)
            {
                throw new NotImplementedException();
            }

            public bool IsPublished(string culture = null)
            {
                throw new NotImplementedException();
            }

            public IPublishedContent Parent { get; }

            public IEnumerable<IPublishedContent> Children => throw new NotImplementedException();

            public IEnumerable<IPublishedContent> ChildrenForAllCultures => throw new NotImplementedException();

            public IEnumerable<IPublishedProperty> Properties => _properties;

            public IPublishedProperty GetProperty(string alias)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
