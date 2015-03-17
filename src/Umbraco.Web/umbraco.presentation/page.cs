using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using Umbraco.Core.Configuration;
using Property = umbraco.cms.businesslogic.property.Property;

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
		/// Initializes a new instance of the <see cref="page"/> class for a yet unpublished document, identified by its <c>id</c> and <c>version</c>.
		/// </summary>
		/// <param name="id">The identifier of the document.</param>
		/// <param name="version">The version to be displayed.</param>
		public page(int id, Guid version)
			: this(new Document(id, version))
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="page"/> class for a yet unpublished document.
		/// </summary>
		/// <param name="document">The document.</param>
		public page(Document document)
		{
			var docParentId = -1;
			try
			{
				docParentId = document.Parent.Id;
			}
			catch (ArgumentException)
			{
				//ignore if no parent
			}

			populatePageData(document.Id,
				document.Text, document.ContentType.Id, document.ContentType.Alias,
				document.User.Name, document.Creator.Name, document.CreateDateTime, document.UpdateDate,
				document.Path, document.Version, docParentId);

			foreach (Property prop in document.GenericProperties)
			{
				string value = prop.Value != null ? prop.Value.ToString() : String.Empty;
				_elements.Add(prop.PropertyType.Alias, value);
			}

			_template = document.Template;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="page"/> class for a published document request.
		/// </summary>
		/// <param name="docreq">The <see cref="PublishedContentRequest"/> pointing to the document.</param>
		/// <remarks>
		/// The difference between creating the page with PublishedContentRequest vs an IPublishedContent item is 
		/// that the PublishedContentRequest takes into account how a template is assigned during the routing process whereas
		/// with an IPublishedContent item, the template id is asssigned purely based on the default.
		/// </remarks>
		internal page(PublishedContentRequest docreq)
		{

			if (!docreq.HasPublishedContent)
				throw new ArgumentException("Document request has no node.", "docreq");
			
			populatePageData(docreq.PublishedContent.Id,
				docreq.PublishedContent.Name, docreq.PublishedContent.DocumentTypeId, docreq.PublishedContent.DocumentTypeAlias,
				docreq.PublishedContent.WriterName, docreq.PublishedContent.CreatorName, docreq.PublishedContent.CreateDate, docreq.PublishedContent.UpdateDate,
				docreq.PublishedContent.Path, docreq.PublishedContent.Version, docreq.PublishedContent.Parent == null ? -1 : docreq.PublishedContent.Parent.Id);

			if (docreq.HasTemplate)
			{

				this._template = docreq.TemplateModel.Id;
				_elements["template"] = _template.ToString();				
			}

			PopulateElementData(docreq.PublishedContent);

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
				doc.Path, doc.Version, doc.Parent == null ? -1 : doc.Parent.Id);

			if (doc.TemplateId > 0)
			{
				//set the template to whatever is assigned to the doc
				_template = doc.TemplateId;
				_elements["template"] = _template.ToString();	
			}			
			
			PopulateElementData(doc);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="page"/> class for a published document.
		/// </summary>
		/// <param name="node">The <c>XmlNode</c> representing the document.</param>
		public page(XmlNode node)
		{			
			populatePageData(node);

		    if (UmbracoConfig.For.UmbracoSettings().WebRouting.DisableAlternativeTemplates == false)
		    {
                // Check for alternative template
		        if (HttpContext.Current.Items[Constants.Conventions.Url.AltTemplate] != null &&
		            HttpContext.Current.Items[Constants.Conventions.Url.AltTemplate].ToString() != String.Empty)
		        {
		            _template =
		                umbraco.cms.businesslogic.template.Template.GetTemplateIdFromAlias(
		                    HttpContext.Current.Items[Constants.Conventions.Url.AltTemplate].ToString());
		            _elements.Add("template", _template.ToString());
		        }
		        else if (helper.Request(Constants.Conventions.Url.AltTemplate) != String.Empty)
		        {
		            _template =
		                umbraco.cms.businesslogic.template.Template.GetTemplateIdFromAlias(
		                    helper.Request(Constants.Conventions.Url.AltTemplate).ToLower());
		            _elements.Add("template", _template.ToString());
		        }
		    }

		    if (_template == 0)
			{
				try
				{
					_template = Convert.ToInt32(node.Attributes.GetNamedItem("template").Value);
					_elements.Add("template", node.Attributes.GetNamedItem("template").Value);
				}
				catch
				{
					HttpContext.Current.Trace.Warn("umbracoPage", "No template defined");
				}
			}

			populateElementData(node);

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
			string path, Guid pageVersion, int parentId)
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
			this._pageVersion = pageVersion;

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
			_elements.Add("pageVersion", pageVersion);
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
			foreach(var p in node.Properties)
			{
				if (_elements.ContainsKey(p.PropertyTypeAlias) == false)				
				{
                    // note: legacy used the raw value (see populating from an Xml node below)
                    // so we're doing the same here, using DataValue. If we use Value then every
                    // value will be converted NOW - including RTEs that may contain macros that
                    // require that the 'page' is already initialized = catch-22.

                    // to properly fix this, we'd need to turn the elements collection into some
                    // sort of collection of lazy values.

					_elements[p.PropertyTypeAlias] = p.DataValue;
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
					LogHelper.Debug<page>(
						string.Format("Aliases must be unique, an element with alias \"{0}\" has already been loaded!", alias), 
						true);					
				}
				else
				{
					_elements[alias] = value;
					LogHelper.Debug<page>(
						string.Format("Load element \"{0}\"", alias),
						true);					
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
            private readonly object _dataValue;
            private readonly IPublishedContent _content;

            public PagePublishedProperty(PublishedPropertyType propertyType, IPublishedContent content)
                : base(propertyType)
            {
                _dataValue = null;
                _content = content;
            }

            public PagePublishedProperty(PublishedPropertyType propertyType, IPublishedContent content, Umbraco.Core.Models.Property property)
                : base(propertyType)
            {
                _dataValue = property.Value;
                _content = content;
            }

            public override bool HasValue
            {
                get { return _dataValue != null && ((_dataValue is string) == false || string.IsNullOrWhiteSpace((string)_dataValue) == false); }
            }

            public override object DataValue
            {
                get { return _dataValue; }
            }

            public override object Value
            {
                get
                {
                    // isPreviewing is true here since we want to preview anyway...
                    const bool isPreviewing = true;
                    var source = PropertyType.ConvertDataToSource(_dataValue, isPreviewing);
                    return PropertyType.ConvertSourceToObject(source, isPreviewing);
                }
            }

            public override object XPathValue
            {
                get { throw new NotImplementedException(); }
            }
        }

        private class PagePublishedContent : IPublishedContent
        {
            private readonly IContent _inner;
            private readonly int _id;
            private readonly string _creatorName;
            private readonly string _writerName;
            private readonly PublishedContentType _contentType;
            private readonly IPublishedProperty[] _properties;
            private readonly IPublishedContent _parent;

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

                _creatorName = _inner.GetCreatorProfile().Name;
                _writerName = _inner.GetWriterProfile().Name;

                _contentType = new PublishedContentType(_inner.ContentType);

                _properties = _contentType.PropertyTypes
                    .Select(x =>
                    {
                        var p = _inner.Properties.SingleOrDefault(xx => xx.Alias == x.PropertyTypeAlias);
                        return p == null ? new PagePublishedProperty(x, this) : new PagePublishedProperty(x, this, p);
                    })
                    .Cast<IPublishedProperty>()
                    .ToArray();

                _parent = new PagePublishedContent(_inner.ParentId);
            }

            public IEnumerable<IPublishedContent> ContentSet
            {
                get { throw new NotImplementedException(); }
            }

            public PublishedContentType ContentType
            {
                get { return _contentType; }
            }

            public int Id
            {
                get { return _id; }
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

            public Guid Version
            {
                get { return _inner.Version; }
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

            public int GetIndex()
            {
                throw new NotImplementedException();
            }

            public IPublishedContent Parent
            {
                get { return _parent; }
            }

            public IEnumerable<IPublishedContent> Children
            {
                get { throw new NotImplementedException(); }
            }

            public ICollection<IPublishedProperty> Properties
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

            public object this[string alias]
            {
                get { throw new NotImplementedException(); }
            }
        }

        #endregion
    }
}
