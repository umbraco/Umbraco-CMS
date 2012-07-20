using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;

namespace umbraco
{
	/// <summary>
	/// Summary description for page.
	/// </summary>
	public class page
	{
		const string TraceCategory = "UmbracoPage";

		#region Private members and properties

		string pageName;
		int parentId;
		string writerName;
		string creatorName;
		string path;
		int nodeType;
		string nodeTypeAlias;
		string[] splitpath;
		DateTime createDate;
		DateTime updateDate;
		int pageID;
		Guid pageVersion;
		int template;

		Hashtable elements = new Hashtable();
		StringBuilder pageContent = new StringBuilder();
		Control pageContentControl = new Control();

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
			HttpContext.Current.Trace.Write(TraceCategory, "Ctor(document)");

			var parent = document.Parent;

			populatePageData(document.Id,
				document.Text, document.ContentType.Id, document.ContentType.Alias,
				document.User.Name, document.Creator.Name, document.CreateDateTime, document.UpdateDate,
				document.Path, document.Version, parent == null ? -1 : parent.Id);

			foreach (Property prop in document.GenericProperties)
			{
				string value = prop.Value != null ? prop.Value.ToString() : String.Empty;
				elements.Add(prop.PropertyType.Alias, value);
			}

			template = document.Template;

			HttpContext.Current.Trace.Write(TraceCategory, string.Format("Loaded \"{0}\" (id={1}, version={2})",
				this.PageName, this.pageID, this.pageVersion));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="page"/> class for a published document.
		/// </summary>
		/// <param name="docreq">The <see cref="DocumentRequest"/> pointing to the document.</param>
		internal page(DocumentRequest docreq)
		{
			HttpContext.Current.Trace.Write(TraceCategory, "Ctor(documentRequest)");

			if (!docreq.HasNode)
				throw new ArgumentException("Document request has no node.", "docreq");

			populatePageData(docreq.Node);

			if (docreq.HasTemplate)
			{
				this.template = docreq.Template.Id;
				elements["template"] = this.template.ToString();
			}

			populateElementData(docreq.Node);

			HttpContext.Current.Trace.Write(TraceCategory, string.Format("Loaded \"{0}\" (id={1}, version={2})",
				this.PageName, this.pageID, this.pageVersion));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="page"/> class for a published document.
		/// </summary>
		/// <param name="node">The <c>XmlNode</c> representing the document.</param>
		public page(XmlNode node)
		{
			HttpContext.Current.Trace.Write(TraceCategory, "Ctor(xmlNode)");

			populatePageData(node);

			// Check for alternative template
			if (HttpContext.Current.Items["altTemplate"] != null &&
				HttpContext.Current.Items["altTemplate"].ToString() != String.Empty)
			{
				template =
					umbraco.cms.businesslogic.template.Template.GetTemplateIdFromAlias(
						HttpContext.Current.Items["altTemplate"].ToString());
				elements.Add("template", template.ToString());
			}
			else if (helper.Request("altTemplate") != String.Empty)
			{
				template =
					umbraco.cms.businesslogic.template.Template.GetTemplateIdFromAlias(helper.Request("altTemplate").ToLower());
				elements.Add("template", template.ToString());
			}
			if (template == 0)
			{
				try
				{
					template = Convert.ToInt32(node.Attributes.GetNamedItem("template").Value);
					elements.Add("template", node.Attributes.GetNamedItem("template").Value);
				}
				catch
				{
					HttpContext.Current.Trace.Warn("umbracoPage", "No template defined");
				}
			}

			populateElementData(node);

			HttpContext.Current.Trace.Write(TraceCategory, string.Format("Loaded \"{0}\" (id={1}, version={2})",
				this.PageName, this.pageID, this.pageVersion));
		}

		#endregion

		#region Initialize

		void populatePageData(int pageID,
			string pageName, int nodeType, string nodeTypeAlias,
			string writerName, string creatorName, DateTime createDate, DateTime updateDate,
			string path, Guid pageVersion, int parentId)
		{
			this.pageID = pageID;
			this.pageName = pageName;
			this.nodeType = nodeType;
			this.nodeTypeAlias = nodeTypeAlias;
			this.writerName = writerName;
			this.creatorName = creatorName;
			this.createDate = createDate;
			this.updateDate = updateDate;
			this.parentId = parentId;
			this.path = path;
			this.splitpath = path.Split(',');
			this.pageVersion = pageVersion;

			// Update the elements hashtable
			elements.Add("pageID", pageID);
			elements.Add("parentID", parentId);
			elements.Add("pageName", pageName);
			elements.Add("nodeType", nodeType);
			elements.Add("nodeTypeAlias", nodeTypeAlias);
			elements.Add("writerName", writerName);
			elements.Add("creatorName", creatorName);
			elements.Add("createDate", createDate);
			elements.Add("updateDate", updateDate);
			elements.Add("path", path);
			elements.Add("splitpath", splitpath);
			elements.Add("pageVersion", pageVersion);
		}

		void populatePageData(XmlNode node)
		{
			String s;
			DateTime dt;
			Guid guid;
			int i;

			if (int.TryParse(attrValue(node, "id"), out i))
				elements["pageID"] = this.pageID = i;

			if ((s = attrValue(node, "nodeName")) != null)
				elements["pageName"] = this.pageName = s;

			if (int.TryParse(attrValue(node, "parentId"), out i))
				elements["parentId"] = this.parentId = i;

			if (int.TryParse(attrValue(node, "nodeType"), out i))
				elements["nodeType"] = this.nodeType = i;
			if ((s = attrValue(node, "nodeTypeAlias")) != null)
				elements["nodeTypeAlias"] = this.nodeTypeAlias = s;

			if ((s = attrValue(node, "writerName")) != null)
				elements["writerName"] = this.writerName = s;
			if ((s = attrValue(node, "creatorName")) != null)
				elements["creatorName"] = this.creatorName = s;

			if (DateTime.TryParse(attrValue(node, "createDate"), out dt))
				elements["createDate"] = this.createDate = dt;
			if (DateTime.TryParse(attrValue(node, "updateDate"), out dt))
				elements["updateDate"] = this.updateDate = dt;

			if (Guid.TryParse(attrValue(node, "pageVersion"), out guid))
				elements["pageVersion"] = this.pageVersion = guid;

			if ((s = attrValue(node, "path")) != null)
			{
				elements["path"] = this.path = s;
				elements["splitpath"] = this.splitpath = path.Split(',');
			}
		}

		string attrValue(XmlNode node, string attributeName)
		{
			var attr = node.Attributes.GetNamedItem(attributeName);
			var value = attr != null ? attr.Value : null;
			return value;
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

				if (elements.ContainsKey(alias))
				{
					HttpContext.Current.Trace.Warn(TraceCategory,
						string.Format("Aliases must be unique, an element with alias \"{0}\" has already been loaded!", alias));
				}
				else
				{
					elements[alias] = value;
					HttpContext.Current.Trace.Write(TraceCategory,
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
				HttpContext.Current.Trace.Write(TraceCategory, string.Format("RenderPage: loading template (id={0})", templateId));
				template templateDesign = new template(templateId);

				HttpContext.Current.Trace.Write(TraceCategory, "RenderPage: template loaded");
				HttpContext.Current.Items["umbPageObject"] = this;

				pageContentControl = templateDesign.ParseWithControls(this);
				pageContent.Append(templateDesign.TemplateContent);
			}
			else
				HttpContext.Current.Trace.Warn(TraceCategory, "RenderPage: no template (id=0)");
		}

		#endregion

		#region Public properties

		public Control PageContentControl
		{
			get { return pageContentControl; }
		}

		public string PageName
		{
			get { return pageName; }
		}

		public int ParentId
		{
			get { return parentId; }
		}

		public string NodeTypeAlias
		{
			get { return nodeTypeAlias; }
		}

		public int NodeType
		{
			get { return nodeType; }
		}

		public string WriterName
		{
			get { return writerName; }
		}

		public string CreatorName
		{
			get { return creatorName; }
		}

		public DateTime CreateDate
		{
			get { return createDate; }
		}

		public DateTime UpdateDate
		{
			get { return updateDate; }
		}

		public int PageID
		{
			get { return pageID; }
		}

		public int Template
		{
			get { return template; }
		}

		public Hashtable Elements
		{
			get { return elements; }
		}

		public string PageContent
		{
			get { return pageContent.ToString(); }
		}

		public string[] SplitPath
		{
			get { return this.splitpath; }
		}

		#endregion

		#region ToString

		public override string ToString()
		{
			return pageName;
		}

		#endregion
	}
}
