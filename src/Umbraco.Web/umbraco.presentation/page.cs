using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Editors;
using Umbraco.Web.Routing;
using Umbraco.Core.Configuration;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;
using Umbraco.Web.Composing;

namespace umbraco
{
    /// <summary>
    /// Summary description for page.
    /// </summary>
    public class page
    {

        #region Private members and properties

        string _pageName;
        int _parentId;
        string _writerName;
        string _creatorName;
        string _path;
        int _nodeType;
        string _nodeTypeAlias;
        string[] _splitpath;
        DateTime _createDate;
        DateTime _updateDate;
        int _pageId;
        Guid _pageVersion;
        readonly int _template;

        readonly Hashtable _elements = new Hashtable();
        readonly StringBuilder _pageContent = new StringBuilder();
        Control _pageContentControl = new Control();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="page"/> class for a published document request.
        /// </summary>
        /// <param name="frequest">The <see cref="PublishedRequest"/> pointing to the document.</param>
        /// <remarks>
        /// The difference between creating the page with PublishedContentRequest vs an IPublishedContent item is
        /// that the PublishedContentRequest takes into account how a template is assigned during the routing process whereas
        /// with an IPublishedContent item, the template id is asssigned purely based on the default.
        /// </remarks>
        internal page(PublishedRequest frequest)
        {

            if (!frequest.HasPublishedContent)
                throw new ArgumentException("Document request has no node.", "frequest");

            populatePageData(frequest.PublishedContent.Id,
                frequest.PublishedContent.Name, frequest.PublishedContent.DocumentTypeId, frequest.PublishedContent.DocumentTypeAlias,
                frequest.PublishedContent.WriterName, frequest.PublishedContent.CreatorName, frequest.PublishedContent.CreateDate, frequest.PublishedContent.UpdateDate,
                frequest.PublishedContent.Path, frequest.PublishedContent.Parent == null ? -1 : frequest.PublishedContent.Parent.Id);

            if (frequest.HasTemplate)
            {

                this._template = frequest.TemplateModel.Id;
                _elements["template"] = _template.ToString();
            }

            PopulateElementData(frequest.PublishedContent);

        }

        /// <summary>
        /// Initializes a new instance of the page for a published document
        /// </summary>
        /// <param name="doc"></param>
        internal page(IPublishedContent doc)
        {
            if (doc == null) throw new ArgumentNullException("doc");

            populatePageData(doc.Id,
                doc.Name, doc.DocumentTypeId, doc.DocumentTypeAlias,
                doc.WriterName, doc.CreatorName, doc.CreateDate, doc.UpdateDate,
                doc.Path, doc.Parent == null ? -1 : doc.Parent.Id);

            if (doc.TemplateId > 0)
            {
                //set the template to whatever is assigned to the doc
                _template = doc.TemplateId;
                _elements["template"] = _template.ToString();
            }

            PopulateElementData(doc);
        }

        /// <summary>
        /// Initializes a new instance of the page for a content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <remarks>This is for <see cref="MacroController"/> usage only.</remarks>
        internal page(IContent content)
            : this(new PagePublishedContent(content))
        { }

        #endregion

        #region Initialize

        void populatePageData(int pageID,
            string pageName, int nodeType, string nodeTypeAlias,
            string writerName, string creatorName, DateTime createDate, DateTime updateDate,
            string path, int parentId)
        {
            this._pageId = pageID;
            this._pageName = pageName;
            this._nodeType = nodeType;
            this._nodeTypeAlias = nodeTypeAlias;
            this._writerName = writerName;
            this._creatorName = creatorName;
            this._createDate = createDate;
            this._updateDate = updateDate;
            this._parentId = parentId;
            this._path = path;
            this._splitpath = path.Split(',');

            // Update the elements hashtable
            _elements.Add("pageID", pageID);
            _elements.Add("parentID", parentId);
            _elements.Add("pageName", pageName);
            _elements.Add("nodeType", nodeType);
            _elements.Add("nodeTypeAlias", nodeTypeAlias);
            _elements.Add("writerName", writerName);
            _elements.Add("creatorName", creatorName);
            _elements.Add("createDate", createDate);
            _elements.Add("updateDate", updateDate);
            _elements.Add("path", path);
            _elements.Add("splitpath", _splitpath);
        }

        void populatePageData(XmlNode node)
        {
            String s;
            DateTime dt;
            Guid guid;
            int i;

            if (int.TryParse(attrValue(node, "id"), out i))
                _elements["pageID"] = this._pageId = i;

            if ((s = attrValue(node, "nodeName")) != null)
                _elements["pageName"] = this._pageName = s;

            if (int.TryParse(attrValue(node, "parentId"), out i))
                _elements["parentId"] = this._parentId = i;

            if (int.TryParse(attrValue(node, "nodeType"), out i))
                _elements["nodeType"] = this._nodeType = i;
            if ((s = attrValue(node, "nodeTypeAlias")) != null)
                _elements["nodeTypeAlias"] = this._nodeTypeAlias = s;

            if ((s = attrValue(node, "writerName")) != null)
                _elements["writerName"] = this._writerName = s;
            if ((s = attrValue(node, "creatorName")) != null)
                _elements["creatorName"] = this._creatorName = s;

            if (DateTime.TryParse(attrValue(node, "createDate"), out dt))
                _elements["createDate"] = this._createDate = dt;
            if (DateTime.TryParse(attrValue(node, "updateDate"), out dt))
                _elements["updateDate"] = this._updateDate = dt;

            if (Guid.TryParse(attrValue(node, "pageVersion"), out guid))
                _elements["pageVersion"] = this._pageVersion = guid;

            if ((s = attrValue(node, "path")) != null)
            {
                _elements["path"] = this._path = s;
                _elements["splitpath"] = this._splitpath = _path.Split(',');
            }
        }

        string attrValue(XmlNode node, string attributeName)
        {
            var attr = node.Attributes.GetNamedItem(attributeName);
            var value = attr != null ? attr.Value : null;
            return value;
        }

        /// <summary>
        /// Puts the properties of the node into the elements table
        /// </summary>
        /// <param name="node"></param>
        void PopulateElementData(IPublishedContent node)
        {
            foreach (var p in node.Properties)
            {
                if (_elements.ContainsKey(p.Alias) == false)
                {
                    // note: legacy used the raw value (see populating from an Xml node below)
                    // so we're doing the same here, using DataValue. If we use Value then every
                    // value will be converted NOW - including RTEs that may contain macros that
                    // require that the 'page' is already initialized = catch-22.

                    // to properly fix this, we'd need to turn the elements collection into some
                    // sort of collection of lazy values.

                    _elements[p.Alias] = p.GetSourceValue();
                }
            }
        }

        void populateElementData(XmlNode node)
        {
            string xpath = "./* [not(@isDoc)]";

            foreach (XmlNode data in node.SelectNodes(xpath))
            {
                // ignore empty elements
                if (data.ChildNodes.Count == 0)
                    continue;

                string alias = data.Name;
                string value = data.FirstChild.Value;

                // moved to PublishedContentRequest + UmbracoModule
                //if (alias == "umbracoRedirect")
                //{
                //    int i;
                //    if (int.TryParse(value, out i))
                //        HttpContext.Current.Response.Redirect(library.NiceUrl(int.Parse(data.FirstChild.Value)), true);
                //}

                if (_elements.ContainsKey(alias))
                {
                    Current.Logger.Debug<page>(
                        string.Format("Aliases must be unique, an element with alias \"{0}\" has already been loaded!", alias));
                }
                else
                {
                    _elements[alias] = value;
                    Current.Logger.Debug<page>(
                        string.Format("Load element \"{0}\"", alias));
                }
            }
        }

        #endregion

        #region Wtf?

        public void RenderPage(int templateId)
        {
            if (templateId != 0)
            {
                template templateDesign = new template(templateId);

                _pageContentControl = templateDesign.ParseWithControls(this);
                _pageContent.Append(templateDesign.TemplateContent);
            }
        }

        #endregion

        #region Public properties

        public Control PageContentControl
        {
            get { return _pageContentControl; }
        }

        public string PageName
        {
            get { return _pageName; }
        }

        public int ParentId
        {
            get { return _parentId; }
        }

        public string NodeTypeAlias
        {
            get { return _nodeTypeAlias; }
        }

        public int NodeType
        {
            get { return _nodeType; }
        }

        public string WriterName
        {
            get { return _writerName; }
        }

        public string CreatorName
        {
            get { return _creatorName; }
        }

        public DateTime CreateDate
        {
            get { return _createDate; }
        }

        public DateTime UpdateDate
        {
            get { return _updateDate; }
        }

        public int PageID
        {
            get { return _pageId; }
        }

        public int Template
        {
            get { return _template; }
        }

        public Hashtable Elements
        {
            get { return _elements; }
        }

        public string PageContent
        {
            get { return _pageContent.ToString(); }
        }

        public string[] SplitPath
        {
            get { return this._splitpath; }
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return _pageName;
        }

        #endregion

        #region PublishedContent

        private class PagePublishedProperty : PublishedPropertyBase
        {
            private readonly object _sourceValue;
            private readonly IPublishedContent _content;

            public PagePublishedProperty(PublishedPropertyType propertyType, IPublishedContent content)
                : base(propertyType, PropertyCacheLevel.Unknown) // cache level is ignored
            {
                _sourceValue = null;
                _content = content;
            }

            public PagePublishedProperty(PublishedPropertyType propertyType, IPublishedContent content, Umbraco.Core.Models.Property property)
                : base(propertyType, PropertyCacheLevel.Unknown) // cache level is ignored
            {
                _sourceValue = property.GetValue();
                _content = content;
            }

            public override bool HasValue(string culture = ".", string segment = ".")
            {
                return _sourceValue != null && ((_sourceValue is string) == false || string.IsNullOrWhiteSpace((string)_sourceValue) == false);
            }

            public override object GetSourceValue(string culture = ".", string segment = ".")
            {
                return _sourceValue;
            }

            public override object GetValue(string culture = ".", string segment = ".")
            {
                // isPreviewing is true here since we want to preview anyway...
                const bool isPreviewing = true;
                var source = PropertyType.ConvertSourceToInter(_content, _sourceValue, isPreviewing);
                return PropertyType.ConvertInterToObject(_content, PropertyCacheLevel.Unknown, source, isPreviewing);
            }

            public override object GetXPathValue(string culture = ".", string segment = ".")
            {
                throw new NotImplementedException();
            }
        }

        private class PagePublishedContent : IPublishedContent
        {
            private readonly IContent _inner;
            private readonly int _id;
            private readonly Guid _key;
            private readonly string _creatorName;
            private readonly string _writerName;
            private readonly PublishedContentType _contentType;
            private readonly IPublishedProperty[] _properties;
            private readonly IPublishedContent _parent;
            private IReadOnlyDictionary<string, PublishedCultureName> _cultureNames;

            private PagePublishedContent(int id)
            {
                _id = id;
            }

            public PagePublishedContent(IContent inner)
            {
                if (inner == null)
                    throw new NullReferenceException("content");

                _inner = inner;
                _id = _inner.Id;
                _key = _inner.Key;

                //TODO: ARGH! need to fix this - this is not good because it uses ApplicationContext.Current
                _creatorName = _inner.GetCreatorProfile().Name;
                _writerName = _inner.GetWriterProfile().Name;

                _contentType = Current.PublishedContentTypeFactory.CreateContentType(_inner.ContentType);

                _properties = _contentType.PropertyTypes
                    .Select(x =>
                    {
                        var p = _inner.Properties.SingleOrDefault(xx => xx.Alias == x.Alias);
                        return p == null ? new PagePublishedProperty(x, this) : new PagePublishedProperty(x, this, p);
                    })
                    .Cast<IPublishedProperty>()
                    .ToArray();

                _parent = new PagePublishedContent(_inner.ParentId);
            }

            public PublishedContentType ContentType
            {
                get { return _contentType; }
            }

            public int Id
            {
                get { return _id; }
            }

            public Guid Key
            {
                get { return _key; }
            }

            public int TemplateId
            {
                get { return _inner.Template == null ? 0 : _inner.Template.Id; }
            }

            public int SortOrder
            {
                get { return _inner.SortOrder; }
            }

            public string Name
            {
                get { return _inner.Name; }
            }

            public IReadOnlyDictionary<string, PublishedCultureName> CultureNames
            {
                get
                {
                    if (!_inner.ContentType.Variations.HasFlag(ContentVariation.CultureNeutral))
                        return null;

                    if (_cultureNames == null)
                    {
                        var d = new Dictionary<string, PublishedCultureName>(StringComparer.InvariantCultureIgnoreCase);
                        foreach (var c in _inner.Names)
                        {
                            d[c.Key] = new PublishedCultureName(c.Value, c.Value.ToUrlSegment());
                        }
                        _cultureNames = d;
                    }
                    return _cultureNames;
                }
            }

            public string UrlName
            {
                get { throw new NotImplementedException(); }
            }

            public string DocumentTypeAlias
            {
                get { return _inner.ContentType.Alias; }
            }

            public int DocumentTypeId
            {
                get { return _inner.ContentTypeId; }
            }

            public string WriterName
            {
                get { return _writerName; }
            }

            public string CreatorName
            {
                get { return _creatorName; }
            }

            public int WriterId
            {
                get { return _inner.WriterId; }
            }

            public int CreatorId
            {
                get { return _inner.CreatorId; }
            }

            public string Path
            {
                get { return _inner.Path; }
            }

            public DateTime CreateDate
            {
                get { return _inner.CreateDate; }
            }

            public DateTime UpdateDate
            {
                get { return _inner.UpdateDate; }
            }

            public int Level
            {
                get { return _inner.Level; }
            }

            public string Url
            {
                get { throw new NotImplementedException(); }
            }

            public PublishedItemType ItemType
            {
                get { return PublishedItemType.Content; }
            }

            public bool IsDraft
            {
                get { throw new NotImplementedException(); }
            }

            public IPublishedContent Parent
            {
                get { return _parent; }
            }

            public IEnumerable<IPublishedContent> Children
            {
                get { throw new NotImplementedException(); }
            }

            public IEnumerable<IPublishedProperty> Properties
            {
                get { return _properties; }
            }

            public IPublishedProperty GetProperty(string alias)
            {
                throw new NotImplementedException();
            }

            public IPublishedProperty GetProperty(string alias, bool recurse)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
