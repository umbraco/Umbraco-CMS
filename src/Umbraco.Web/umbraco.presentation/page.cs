using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;

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

		/// <summary>
		/// Returns the trace context to use for debugging, this is for backwards compatibility as people may be execting some of this
		/// information in the TraceContext.
		/// </summary>
		private TraceContext TraceContext
		{
			get
			{
				if (HttpContext.Current == null)
				{
					return null;
				}
				return HttpContext.Current.Trace;
			}
		}

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
		/// Initializes a new instance of the <see cref="page"/> class for a published document.
		/// </summary>
		/// <param name="docreq">The <see cref="DocumentRequest"/> pointing to the document.</param>
		internal page(DocumentRequest docreq)
		{

			if (!docreq.HasNode)
				throw new ArgumentException("Document request has no node.", "docreq");
			
			populatePageData(docreq.Document.Id,
				docreq.Document.Name, docreq.Document.DocumentTypeId, docreq.Document.DocumentTypeAlias,
				docreq.Document.WriterName, docreq.Document.CreatorName, docreq.Document.CreateDate, docreq.Document.UpdateDate,
				docreq.Document.Path, docreq.Document.Version, docreq.Document.Parent == null ? -1 : docreq.Document.Parent.Id);

			if (docreq.HasTemplate)
			{

				this._template = docreq.Template.Id;
				_elements["template"] = _template.ToString();				
			}

			PopulateElementData(docreq.Document);

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="page"/> class for a published document.
		/// </summary>
		/// <param name="node">The <c>XmlNode</c> representing the document.</param>
		public page(XmlNode node)
		{			
			populatePageData(node);

			// Check for alternative template
			if (HttpContext.Current.Items["altTemplate"] != null &&
				HttpContext.Current.Items["altTemplate"].ToString() != String.Empty)
			{
				_template =
					umbraco.cms.businesslogic.template.Template.GetTemplateIdFromAlias(
						HttpContext.Current.Items["altTemplate"].ToString());
				_elements.Add("template", _template.ToString());
			}
			else if (helper.Request("altTemplate") != String.Empty)
			{
				_template =
					umbraco.cms.businesslogic.template.Template.GetTemplateIdFromAlias(helper.Request("altTemplate").ToLower());
				_elements.Add("template", _template.ToString());
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
		void PopulateElementData(IDocument node)
		{
			foreach(var p in node.Properties)
			{
				if (!_elements.ContainsKey(p.Alias))				
				{
					_elements[p.Alias] = p.Value;					
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

				// moved to DocumentRequest + UmbracoModule
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
						TraceContext);					
				}
				else
				{
					_elements[alias] = value;
					LogHelper.Debug<page>(
						string.Format("Load element \"{0}\"", alias),
						TraceContext);					
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
			
				HttpContext.Current.Items["umbPageObject"] = this;

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
	}
}
