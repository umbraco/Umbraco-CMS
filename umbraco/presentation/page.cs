using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;

using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;

namespace umbraco
{
    /// <summary>
    /// Summary description for page.
    /// </summary>
    public class page
    {
        #region private members and properties

        private String pageName;
        private int parentId;
        private String writerName;
        private String path;
        private int nodeType;
        private String nodeTypeAlias;
        private String[] splitpath;
        private DateTime createDate;
        private DateTime updateDate;
        private int pageID;
        private Guid pageVersion;
        private int template;
        private Hashtable elements = new Hashtable();
        private StringBuilder pageContent = new StringBuilder();
        private Control pageContentControl = new Control();

        #endregion

        #region init

        /// <summary>
        /// Constructor for creating a page in view mode (with viable content that's not yet published)
        /// </summary>
        /// <param name="Id">The identifier of the page</param>
        /// <param name="Version">The version to be displayed</param>
        public page(int Id, Guid Version)
        {
            pageID = Id;

            // create document object
            Document d = new Document(Id, Version);

            int tmpParentId = -1;
            try
            {
                tmpParentId = d.Parent.Id;
            }
            catch
            {
            }
            // create page
            populatePageData(Id, d.Text, d.ContentType.Id, d.ContentType.Alias, d.User.Name, d.CreateDateTime,
                             d.UpdateDate, d.Path, d.Version, tmpParentId);

            // update page elements
            foreach (Property p in d.getProperties)
            {
                string sValue = p.Value!=null ? p.Value.ToString() : String.Empty;
                elements.Add(p.PropertyType.Alias, sValue);
            }
            template = d.Template;
            HttpContext.Current.Trace.Write("umbracoPage",
                                            "Pagedata loaded for " + pageName + " (ID: " + pageID.ToString() +
                                            ", Version: " + pageVersion.ToString() + ")");
            //			RenderPage(template);
        }

        private void populatePageData(int pageID, string pageName, int nodeType, string nodeTypeAlias, string writerName,
                                      DateTime createDate, DateTime updateDate, string path, Guid pageVersion,
                                      int parentId)
        {
            this.pageID = pageID;
            this.pageName = pageName;
            this.nodeType = nodeType;
            this.nodeTypeAlias = nodeTypeAlias;
            this.writerName = writerName;
            this.createDate = createDate;
            this.updateDate = updateDate;
            this.parentId = parentId;
            this.path = path;
            splitpath = path.Split(',');
            this.pageVersion = pageVersion;

            // Update the elements hashtable
            elements.Add("pageID", pageID);
            elements.Add("parentID", parentId);
            elements.Add("pageName", pageName);
            elements.Add("nodeType", nodeType);
            elements.Add("nodeTypeAlias", nodeTypeAlias);
            elements.Add("writerName", writerName);
            elements.Add("createDate", createDate);
            elements.Add("updateDate", updateDate);
            elements.Add("path", path);
            elements.Add("splitpath", splitpath);
            elements.Add("pageVersion", pageVersion);
        }

        public page(XmlNode xmlNode)
        {
            pageID = Convert.ToInt32(xmlNode.Attributes.GetNamedItem("id").Value);
            elements.Add("pageID", pageID);
            try
            {
                pageName = xmlNode.Attributes.GetNamedItem("nodeName").Value;
                elements.Add("pageName", pageName);
            }
            catch
            {
            }
            try
            {
                parentId = int.Parse(xmlNode.Attributes.GetNamedItem("parentID").Value);
                elements.Add("parentID", parentId);
            }
            catch
            {
            }

            try
            {
                nodeType = Convert.ToInt32(xmlNode.Attributes.GetNamedItem("nodeType").Value);
                nodeTypeAlias = xmlNode.Attributes.GetNamedItem("nodeTypeAlias").Value;
                elements.Add("nodeType", nodeType);
                elements.Add("nodeTypeAlias", nodeTypeAlias);
            }
            catch
            {
            }
            try
            {
                writerName = xmlNode.Attributes.GetNamedItem("writerName").Value;
                elements.Add("writerName", writerName);
            }
            catch
            {
            }
            try
            {
                createDate = Convert.ToDateTime(xmlNode.Attributes.GetNamedItem("createDate").Value);
                elements.Add("createDate", createDate);
            }
            catch
            {
            }
            try
            {
                updateDate = Convert.ToDateTime(xmlNode.Attributes.GetNamedItem("updateDate").Value);
                elements.Add("updateDate", updateDate);
            }
            catch
            {
            }
            try
            {
                pageVersion = new Guid(xmlNode.Attributes.GetNamedItem("version").Value);
                elements.Add("pageVersion", pageVersion);
            }
            catch
            {
            }
            try
            {
                path = xmlNode.Attributes.GetNamedItem("path").Value;
                elements.Add("path", path);
                splitpath = path.Split(',');
                elements.Add("splitpath", splitpath);
            }
            catch
            {
            }

            // Check for alternative template
            if (HttpContext.Current.Items["altTemplate"] != null &&
                HttpContext.Current.Items["altTemplate"].ToString() != "")
            {
                template =
                    umbraco.cms.businesslogic.template.Template.GetTemplateIdFromAlias(
                        HttpContext.Current.Items["altTemplate"].ToString());
                elements.Add("template", template.ToString());
            }
            else if (helper.Request("altTemplate") != "")
            {
                template =
                    umbraco.cms.businesslogic.template.Template.GetTemplateIdFromAlias(helper.Request("altTemplate").ToLower());
                elements.Add("template", template.ToString());
            }
            if (template == 0)
            {
                try
                {
                    template = Convert.ToInt32(xmlNode.Attributes.GetNamedItem("template").Value);
                    elements.Add("template", xmlNode.Attributes.GetNamedItem("template").Value);
                }
                catch
                {
                    HttpContext.Current.Trace.Warn("umbracoPage", "No template defined");
                }
            }

            // Load all page elements
            foreach (XmlNode dataNode in xmlNode.SelectNodes("./data"))
            {
                if (dataNode == null)
                    continue;
                // Only add those data-elements who has content (ie. a childnode)
                if (dataNode.FirstChild != null)
                {
                    // Check for redirects
                    if (helper.IsNumeric(dataNode.FirstChild.Value) &&
                        dataNode.Attributes.GetNamedItem("alias").Value == "umbracoRedirect" &&
                        int.Parse(dataNode.FirstChild.Value) > 0)
                    {
                        HttpContext.Current.Response.Redirect(library.NiceUrl(int.Parse(dataNode.FirstChild.Value)),
                                                              true);
                    }
                    else
                    {
                        if (elements.ContainsKey(dataNode.Attributes.GetNamedItem("alias").Value))
                            HttpContext.Current.Trace.Warn("umbracoPage",
                                                           "Aliases must be unique, an element with alias '" +
                                                           dataNode.Attributes.GetNamedItem("alias").Value +
                                                           "' has already been loaded!");
                        else
                        {
                            elements.Add(dataNode.Attributes.GetNamedItem("alias").Value,
                                         dataNode.FirstChild.Value
                                );
                            HttpContext.Current.Trace.Write("umbracoPage",
                                                            "Element loaded: " +
                                                            dataNode.Attributes.GetNamedItem("alias").Value);
                        }
                    }
                }
            }

            HttpContext.Current.Trace.Write("umbracoPage",
                                            "Pagedata loaded for " + pageName + " (ID: " + pageID.ToString() +
                                            ", Version: " + pageVersion.ToString() + ")");

            // Save to cache
            //			System.Web.HttpRuntime.Cache.Insert("umbPage" + pageID.ToString(), this);
        }

        public void RenderPage(int Template)
        {
            if (Template != 0)
            {
                HttpContext.Current.Trace.Write("umbracoPage", "Loading template (ID: " + Template + ")");
                template templateDesign = new template(Template);

                HttpContext.Current.Trace.Write("page", "Template loaded");
                HttpContext.Current.Items["umbPageObject"] = this;

                pageContentControl = templateDesign.ParseWithControls(this);
                pageContent.Append(templateDesign.TemplateContent);
            }
            else
                HttpContext.Current.Trace.Warn("page.RenderPage", "No template defined (value=0)");
        }

        public string GetCulture()
        {
            if (Domain.Exists(HttpContext.Current.Request.ServerVariables["SERVER_NAME"]))
            {
                Domain d = Domain.GetDomain(HttpContext.Current.Request.ServerVariables["SERVER_NAME"]);
                return d.Language.CultureAlias;
            }
            else
            {
                for (int i = splitpath.Length - 1; i > 0; i--)
                {
                    if (Domain.GetDomainsById(int.Parse(splitpath[i])).Length > 0)
                    {
                        return Domain.GetDomainsById(int.Parse(splitpath[i]))[0].Language.CultureAlias;
                    }
                }
            }
            return "";
        }

        #endregion

        #region public properties

        public Control PageContentControl
        {
            get { return pageContentControl; }
        }

        public String PageName
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

        public String WriterName
        {
            get { return writerName; }
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

        public String PageContent
        {
            get { return pageContent.ToString(); }
        }

        public override string ToString()
        {
            return pageName;
        }

        #endregion
    }
}