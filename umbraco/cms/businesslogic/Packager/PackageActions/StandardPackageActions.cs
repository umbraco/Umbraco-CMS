using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
using umbraco.IO;

namespace umbraco.cms.businesslogic.packager.standardPackageActions {
    /*Build in standard actions */

    /// <summary>
    /// This class implements the IPackageAction Interface, used to execute code when packages are installed.
    /// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
    /// </summary>
    public class addApplicationTree : umbraco.interfaces.IPackageAction {

        #region IPackageAction Members

        /// <summary>
        /// Executes the specified package action.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns></returns>
        /// <example><code>
        /// <Action runat="install" [undo="false"] alias="addApplicationTree" silent="[true/false]"  initialize="[true/false]" sortOrder="1" 
        /// applicationAlias="appAlias" treeAlias="myTree" treeTitle="My Tree" iconOpened="folder_o.gif" iconClosed="folder.gif"
        /// assemblyName="umbraco" treeHandlerType="treeClass" action="alert('you clicked my tree')"/>
        /// </code></example>
        public bool Execute(string packageName, XmlNode xmlData) {
            bool silent = bool.Parse(xmlData.Attributes["silent"].Value);
            bool initialize = bool.Parse(xmlData.Attributes["initialize"].Value);
            byte sortOrder = byte.Parse(xmlData.Attributes["sortOrder"].Value);

            string applicationAlias = xmlData.Attributes["applicationAlias"].Value;
            string treeAlias = xmlData.Attributes["treeAlias"].Value;
            string treeTitle = xmlData.Attributes["treeTitle"].Value;
            string iconOpened = xmlData.Attributes["iconOpened"].Value;
            string iconClosed = xmlData.Attributes["iconClosed"].Value;

            string assemblyName = xmlData.Attributes["assemblyName"].Value;
            string type = xmlData.Attributes["treeHandlerType"].Value;
            string action = xmlData.Attributes["action"].Value;


            BusinessLogic.ApplicationTree.MakeNew(silent, initialize, sortOrder, applicationAlias, treeAlias, treeTitle, iconClosed, iconOpened, assemblyName, type, action);

            return true;
        }

        /// <summary>
        /// Undoes the action
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns></returns>
        public bool Undo(string packageName, XmlNode xmlData) {
            string treeAlias = xmlData.Attributes["treeAlias"].Value;
            BusinessLogic.ApplicationTree.getByAlias(treeAlias).Delete();
            return true;
        }

        /// <summary>
        /// Action alias.
        /// </summary>
        /// <returns></returns>
        public string Alias() {
            return "addApplicationTree";
        }

        #endregion

      

        public XmlNode SampleXml() {

            string sample = "<Action runat=\"install\" undo=\"true/false\" alias=\"addApplicationTree\" silent=\"true/false\"  initialize=\"true/false\" sortOrder=\"1\" applicationAlias=\"appAlias\" treeAlias=\"myTree\" treeTitle=\"My Tree\" iconOpened=\"folder_o.gif\" iconClosed=\"folder.gif\" assemblyName=\"umbraco\" treeHandlerType=\"treeClass\" action=\"alert('you clicked my tree')\"/>";
            return helper.parseStringToXmlNode(sample);
        }

      
    }

    /// <summary>
    /// This class implements the IPackageAction Interface, used to execute code when packages are installed.
    /// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
    /// </summary>
    public class addApplication : umbraco.interfaces.IPackageAction {

        #region IPackageAction Members

        /// <summary>
        /// Installs a new application in umbraco.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <example><code>
        /// <Action runat="install" [undo="false"] alias="addApplication" appName="Application Name"  appAlias="myApplication" appIcon="application.gif"/>
        /// </code></example>
        /// <returns>true if successfull</returns>
        public bool Execute(string packageName, XmlNode xmlData) {
            string name = xmlData.Attributes["appName"].Value;
            string alias = xmlData.Attributes["appAlias"].Value;
            string icon = xmlData.Attributes["appIcon"].Value;

            BusinessLogic.Application.MakeNew(name, alias, icon);

            return true;
        }

        public bool Undo(string packageName, XmlNode xmlData) {
            string alias = xmlData.Attributes["appAlias"].Value;
            BusinessLogic.Application.getByAlias(alias).Delete();
            return true;
        }
        /// <summary>
        /// Action alias.
        /// </summary>
        /// <returns></returns>
        public string Alias() {
            return "addApplication";
        }

        #endregion

        public XmlNode SampleXml() {
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class addDashboardSection : umbraco.interfaces.IPackageAction {
        #region IPackageAction Members

        /// <summary>
        /// Installs a dashboard section. This action reuses the action XML, so it has to be valid dashboard markup.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns>true if successfull</returns>
        /// <example>
        /// <code>
        /// <Action runat="install" [undo="false"] alias="addDashboardSection" dashboardAlias="MyDashboardSection">
        ///     <section>
	    ///         <areas>
		///         <area>default</area>
		///         <area>content</area>
	    ///         </areas>
        ///	        <tab caption="Last Edits">
		///             <control>/usercontrols/dashboard/latestEdits.ascx</control>
        ///             <control>/usercontrols/umbracoBlog/dashboardBlogPostCreate.ascx</control>
	    ///         </tab>
	    ///         <tab caption="Create blog post">
		///             <control>/usercontrols/umbracoBlog/dashboardBlogPostCreate.ascx</control>
	    ///         </tab>
        ///     </section>
        /// </Action>
        /// </code>
        /// </example>
        public bool Execute(string packageName, XmlNode xmlData) {
            //this will need a complete section node to work... 

            if (xmlData.HasChildNodes) {
                string sectionAlias = xmlData.Attributes["dashboardAlias"].Value;
                string dbConfig = SystemFiles.DashboardConfig;

                XmlNode section = xmlData.SelectSingleNode("./section");
                XmlDocument dashboardFile = xmlHelper.OpenAsXmlDocument(dbConfig);
                XmlNode importedSection = dashboardFile.ImportNode(section, true);
                XmlAttribute alias = xmlHelper.addAttribute(dashboardFile, "alias", sectionAlias);
                importedSection.Attributes.Append(alias);

                dashboardFile.DocumentElement.AppendChild( dashboardFile.ImportNode(section, true) );

                dashboardFile.Save(IOHelper.MapPath(dbConfig));

                return true;
            }

            return false;
        }


        public string Alias() {
            return "addDashboardSection";
        }

        public bool Undo(string packageName, XmlNode xmlData) {

            string sectionAlias = xmlData.Attributes["dashboardAlias"].Value;
            string dbConfig = SystemFiles.DashboardConfig;
            XmlDocument dashboardFile = xmlHelper.OpenAsXmlDocument(dbConfig);
            
            XmlNode section = dashboardFile.SelectSingleNode("//section [@alias = '" + sectionAlias + "']");

            if(section != null){
                dashboardFile.RemoveChild(section);
                dashboardFile.Save(IOHelper.MapPath(dbConfig));
            }
            
            return true;    
        }

        #endregion

        public XmlNode SampleXml() {
            throw new NotImplementedException();
        }

    }
    /// <summary>
    /// This class implements the IPackageAction Interface, used to execute code when packages are installed.
    /// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
    /// </summary>
    public class allowDoctype : umbraco.interfaces.IPackageAction {

        #region IPackageAction Members

        /// <summary>
        /// Allows a documentType to be created below another documentType.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <example><code>
        /// <Action runat="install" alias="allowDocumenttype" documentTypeAlias="MyNewDocumentType" parentDocumentTypeAlias="HomePage"  />
        /// </code></example>
        /// <returns>Returns true on success</returns>
        public bool Execute(string packageName, XmlNode xmlData) {
            string doctypeName = xmlData.Attributes["documentTypeAlias"].Value;
            string parentDoctypeName = xmlData.Attributes["parentDocumentTypeAlias"].Value;

            cms.businesslogic.ContentType ct = cms.businesslogic.ContentType.GetByAlias(doctypeName);
            cms.businesslogic.ContentType parentct = cms.businesslogic.ContentType.GetByAlias(parentDoctypeName);

            if (ct != null && parentct != null) {
                bool containsId = false;
                ArrayList tmp = new ArrayList();

                foreach (int i in parentct.AllowedChildContentTypeIDs.ToList()) {
                    tmp.Add(i);
                    if (i == ct.Id)
                        containsId = true;
                }

                if (!containsId) {

                    int[] ids = new int[tmp.Count + 1];
                    for (int i = 0; i < tmp.Count; i++) ids[i] = (int)tmp[i];
                    ids[ids.Length - 1] = ct.Id;

                    parentct.AllowedChildContentTypeIDs = ids;
                    parentct.Save();
                    return true;
                }
            }
            return false;
        }

        //this has no undo.
        /// <summary>
        /// This action has no undo.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns></returns>
        public bool Undo(string packageName, XmlNode xmlData) {
            return true;
        }

        /// <summary>
        /// Action Alias.
        /// </summary>
        /// <returns></returns>
        public string Alias() {
            return "allowDocumenttype";
        }

        #endregion

        public XmlNode SampleXml() {
            throw new NotImplementedException();
        }

    }

    public class addXsltExtension : umbraco.interfaces.IPackageAction {
        #region IPackageAction Members

        public bool Execute(string packageName, XmlNode xmlData) {

            string _assembly = xmlData.Attributes["assembly"].Value;
            string _type = xmlData.Attributes["type"].Value;
            string _alias = xmlData.Attributes["extensionAlias"].Value;
            string xeConfig = SystemFiles.XsltextensionsConfig;

            XmlDocument xdoc = new XmlDocument();
            xdoc.PreserveWhitespace = true;
            xdoc = xmlHelper.OpenAsXmlDocument(xeConfig);

            XmlNode xn = xdoc.SelectSingleNode("//XsltExtensions");

            if (xn != null) {
                bool insertExt = true;
                if (xn.HasChildNodes) {
                    foreach (XmlNode ext in xn.SelectNodes("//ext")) {
                        if (ext.Attributes["alias"] != null && ext.Attributes["alias"].Value == _alias)
                            insertExt = false;
                    }
                }
                if (insertExt) {
                    XmlNode newExt = umbraco.xmlHelper.addTextNode(xdoc, "ext", "");
                    newExt.Attributes.Append(umbraco.xmlHelper.addAttribute(xdoc, "assembly", _assembly));
                    newExt.Attributes.Append(umbraco.xmlHelper.addAttribute(xdoc, "type", _type));
                    newExt.Attributes.Append(umbraco.xmlHelper.addAttribute(xdoc, "alias", _alias));
                    xn.AppendChild(newExt);


                    xdoc.Save(IOHelper.MapPath(xeConfig));
                    return true;
                }
            }
            return false;
        }

        public string Alias() {
            return "addXsltExtension";
        }

        public bool Undo(string packageName, XmlNode xmlData) {
            string _assembly = xmlData.Attributes["assembly"].Value;
            string _type = xmlData.Attributes["type"].Value;
            string _alias = xmlData.Attributes["extensionAlias"].Value;
            string xeConfig = SystemFiles.XsltextensionsConfig;

            XmlDocument xdoc = new XmlDocument();
            xdoc.PreserveWhitespace = true;
            xdoc = xmlHelper.OpenAsXmlDocument(xeConfig);

            XmlNode xn = xdoc.SelectSingleNode("//XsltExtensions");

            if (xn != null) {
                bool inserted = false;
                if (xn.HasChildNodes) {
                    foreach (XmlNode ext in xn.SelectNodes("//ext")) {
                        if (ext.Attributes["alias"] != null && ext.Attributes["alias"].Value == _alias) {
                            xn.RemoveChild(ext);
                            inserted = true;
                        }
                    }
                }

                if (inserted) {
                    xdoc.Save(IOHelper.MapPath(xeConfig));
                    return true;
                }
            }
            return false;
        }

        #endregion

        public XmlNode SampleXml() {
            throw new NotImplementedException();
        }

    }

    public class addRestExtension : umbraco.interfaces.IPackageAction {
        #region IPackageAction Members

        public bool Execute(string packageName, XmlNode xmlData) {

            XmlNodeList _newExts = xmlData.SelectNodes("//ext");

            if (_newExts.Count > 0) {

                string reConfig = SystemFiles.RestextensionsConfig;

                XmlDocument xdoc = new XmlDocument();
                xdoc.PreserveWhitespace = true;
                xdoc.Load(reConfig);

                XmlNode xn = xdoc.SelectSingleNode("//RestExtensions");

                if (xn != null) {
                    for (int i = 0; i < _newExts.Count; i++) {
                        XmlNode newExt = _newExts[i];
                        string _alias = newExt.Attributes["alias"].Value;
                        
                        bool insertExt = true;
                        if (xn.HasChildNodes) {
                            foreach (XmlNode ext in xn.SelectNodes("//ext")) {
                                if (ext.Attributes["alias"] != null && ext.Attributes["alias"].Value == _alias)
                                    insertExt = false;
                            }
                        }

                        if (insertExt) {
                            xn.AppendChild(xdoc.ImportNode(newExt, true));
                        }
                    }

                    xdoc.Save(IOHelper.MapPath(reConfig));
                    return true;
                }
            }
            return false;
        }

        public string Alias() {
            return "addRestExtension";
        }

        public bool Undo(string packageName, XmlNode xmlData) {

            XmlNodeList _newExts = xmlData.SelectNodes("//ext");
            
            if (_newExts.Count > 0) {
                string reConfig = SystemFiles.RestextensionsConfig;

                XmlDocument xdoc = new XmlDocument();
                xdoc.PreserveWhitespace = true;
                xdoc.Load(reConfig);

                XmlNode xn = xdoc.SelectSingleNode("//RestExtensions");

                if (xn != null) {
                    bool inserted = false;

                    for (int i = 0; i < _newExts.Count; i++) {
                        XmlNode newExt = _newExts[i];
                        string _alias = newExt.Attributes["alias"].Value;
                        if (xn.HasChildNodes) {
                            foreach (XmlNode ext in xn.SelectNodes("//ext")) {
                                if (ext.Attributes["alias"] != null && ext.Attributes["alias"].Value == _alias) {
                                    xn.RemoveChild(ext);
                                    inserted = true;                                
                                }
                            }
                        }
                    }

                    if (inserted) {
                        xdoc.Save(IOHelper.MapPath(reConfig));
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        public XmlNode SampleXml() {
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// This class implements the IPackageAction Interface, used to execute code when packages are installed.
    /// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
    /// </summary>
    public class moveRootDocument : umbraco.interfaces.IPackageAction {
        #region IPackageAction Members

        /// <summary>
        /// Executes the specified package action.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <example><code>
        /// <Action runat="install" alias="moveRootDocument" documentName="News" parentDocumentType="Home"  />
        /// </code></example>
        /// <returns>True if executed succesfully</returns>
        public bool Execute(string packageName, XmlNode xmlData) {

            string documentName = xmlData.Attributes["documentName"].Value;
            string parentDocumentType = xmlData.Attributes["parentDocumentType"].Value;
            string parentDocumentName = "";

            if (xmlData.Attributes["parentDocumentName"] != null)
                parentDocumentName = xmlData.Attributes["parentDocumentName"].Value;

            int parentDocid = 0;

            ContentType ct = ContentType.GetByAlias(parentDocumentType);
            Content[] docs = web.Document.getContentOfContentType(ct);

            if (docs.Length > 0) {
                if (String.IsNullOrEmpty(parentDocumentName))
                    parentDocid = docs[0].Id;
                else {
                    foreach (Content doc in docs) {
                        if (doc.Text == parentDocumentName)
                            parentDocid = doc.Id;
                    }
                }
            }

            if (parentDocid > 0) {
                web.Document[] rootDocs = web.Document.GetRootDocuments();

                foreach (web.Document rootDoc in rootDocs) {
                    if (rootDoc.Text == documentName) {
                        rootDoc.Move(parentDocid);
                        rootDoc.PublishWithSubs(new umbraco.BusinessLogic.User(0));
                    }
                }
            }


            return true;
        }

        //this has no undo.
        /// <summary>
        /// This action has no undo.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns></returns>
        public bool Undo(string packageName, XmlNode xmlData) {
            return true;
        }

        /// <summary>
        /// Action alias
        /// </summary>
        /// <returns></returns>
        public string Alias() {
            return "moveRootDocument";
        }

        #endregion

        public XmlNode SampleXml() {
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// This class implements the IPackageAction Interface, used to execute code when packages are installed.
    /// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
    /// </summary>
    public class publishRootDocument : umbraco.interfaces.IPackageAction {
        #region IPackageAction Members

        /// <summary>
        /// Executes the specified package action.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <example>
        /// <Action runat="install" alias="publishRootDocument" documentName="News"  />
        /// </example>
        /// <returns>True if executed succesfully</returns>
        public bool Execute(string packageName, XmlNode xmlData) {
            
            string documentName = xmlData.Attributes["documentName"].Value;

            int parentDocid = 0;

            web.Document[] rootDocs = web.Document.GetRootDocuments();

            foreach (web.Document rootDoc in rootDocs) {
                if (rootDoc.Text.Trim() == documentName.Trim() && rootDoc != null && rootDoc.ContentType != null) {
                    
                    rootDoc.PublishWithChildrenWithResult(umbraco.BusinessLogic.User.GetUser(0));

                    
                    break;
                }
            }
            return true;
        }

        //this has no undo.
        /// <summary>
        /// This action has no undo.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns></returns>
        public bool Undo(string packageName, XmlNode xmlData) {
            return true;
        }

        /// <summary>
        /// Action alias
        /// </summary>
        /// <returns></returns>
        public string Alias() {
            return "publishRootDocument";
        }
        #endregion

        public XmlNode SampleXml() {
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// This class implements the IPackageAction Interface, used to execute code when packages are installed.
    /// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
    /// addStringToHtmlElement adds a string to specific HTML element in a specific template, and can either append or prepend it.
    /// It uses the action xml node to do this, exemple action xml node:
    /// <Action runat="install" alias="addStringToHtmlElement" templateAlias="news" htmlElementId="newsSection" position="end"><![CDATA[hello world!]]></action>
    /// The above will add the string "hello world!" to the first html element with the id "newsSection" in the template "news"
    /// </summary>
    public class addStringToHtmlElement : umbraco.interfaces.IPackageAction {
        #region IPackageAction Members

        /// <summary>
        /// Executes the specified package action.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <example><code><code> 
        ///     <Action runat="install" alias="addStringToHtmlElement" templateAlias="news" htmlElementId="newsSection" position="[beginning/end"><![CDATA[hello world!]]></action>
        /// </code></code></example>
        /// <returns>True if executed successfully</returns>
        public bool Execute(string packageName, XmlNode xmlData) {

            BusinessLogic.Log.Add(BusinessLogic.LogTypes.Error, BusinessLogic.User.GetUser(0), -1, "executing addStringToHtmlElement");

            string templateAlias = xmlData.Attributes["templateAlias"].Value;
            string htmlElementId = xmlData.Attributes["htmlElementId"].Value;
            string position = xmlData.Attributes["position"].Value;
            string value = xmlHelper.GetNodeValue(xmlData);
            template.Template tmp = template.Template.GetByAlias(templateAlias);
            
            if (UmbracoSettings.UseAspNetMasterPages)
                value = tmp.EnsureMasterPageSyntax(value);

            _addStringToHtmlElement(tmp, value, templateAlias, htmlElementId, position);

            return true;
        }


        /// <summary>
        /// Undoes the addStringToHtml Execute() method, by removing the same string from the same template.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns></returns>
        public bool Undo(string packageName, XmlNode xmlData) {
            string templateAlias = xmlData.Attributes["templateAlias"].Value;
            string htmlElementId = xmlData.Attributes["htmlElementId"].Value;
            string value = xmlHelper.GetNodeValue(xmlData);
            template.Template tmp = template.Template.GetByAlias(templateAlias);

            if (UmbracoSettings.UseAspNetMasterPages)
                value = tmp.EnsureMasterPageSyntax(value);

            _removeStringFromHtmlElement(tmp, value, templateAlias, htmlElementId);
            return true;
        }

        /// <summary>
        /// Action alias.
        /// </summary>
        /// <returns></returns>
        public string Alias() {
            return "addStringToHtmlElement";
        }

        private void _addStringToHtmlElement(template.Template tmp, string value, string templateAlias, string htmlElementId, string position) {
            bool hasAspNetContentBeginning = false;
            string design = "";
            string directive = "";

            if (tmp != null) {
                try {
                    XmlDocument templateXml = new XmlDocument();
                    templateXml.PreserveWhitespace = true;

                    //Make sure that directive is remove before hacked non html4 compatiple replacement action... 
                    design = tmp.Design;
                    

                    splitDesignAndDirective(ref design, ref directive);

                    //making sure that the template xml has a root node...
                    if (tmp.MasterTemplate > 0)
                        templateXml.LoadXml( helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, "<root>" + design + "</root>", true));
                    else
                        templateXml.LoadXml(helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, design, true));

                    XmlNode xmlElement = templateXml.SelectSingleNode("//* [@id = '" + htmlElementId + "']");

                    if (xmlElement != null) {

                        if (position == "beginning") {
                            xmlElement.InnerXml = "\n" + helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, value, true) + "\n" + xmlElement.InnerXml;
                        } else {
                            xmlElement.InnerXml = xmlElement.InnerXml + "\n" + helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, value, true) + "\n";
                        }
                    }

                    tmp.Design = directive + "\n" + helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, templateXml.OuterXml, false);
                    tmp.Save();
                } catch (Exception ex) {
                    umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());
                }
            } else {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "template not found");
            }
        }

        private void _removeStringFromHtmlElement(template.Template tmp, string value, string templateAlias, string htmlElementId) {
            bool hasAspNetContentBeginning = false;
            string design = "";
            string directive = "";

            
            if (tmp != null) {
                try {
                    XmlDocument templateXml = new XmlDocument();
                    templateXml.PreserveWhitespace = true;
                  
                    //Make sure that directive is remove before hacked non html4 compatiple replacement action... 
                    design = tmp.Design;
                    splitDesignAndDirective(ref design, ref directive);

                    //making sure that the template xml has a root node...
                    if (tmp.MasterTemplate > 0)
                        templateXml.LoadXml( helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, "<root>" + design + "</root>", true));
                    else
                        templateXml.LoadXml( helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, design, true));

                   XmlNode xmlElement = templateXml.SelectSingleNode("//* [@id = '" + htmlElementId + "']");
                    
 

                    if (xmlElement != null) {
                        string repValue = helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, value, true);
                        xmlElement.InnerXml = xmlElement.InnerXml.Replace(repValue , "");
                    }

                    tmp.Design = directive + "\n" + helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, templateXml.OuterXml, false);
                    tmp.Save();
                } catch (Exception ex) {
                    umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString() );
                }
            } else {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "template not found");
            }
        }

       

        private void splitDesignAndDirective(ref string design, ref string directive) {
            if (design.StartsWith("<%@")) {
                directive = design.Substring(0, design.IndexOf("%>") + 2).Trim(Environment.NewLine.ToCharArray());
                design = design.Substring(design.IndexOf("%>") + 3).Trim(Environment.NewLine.ToCharArray());
            }
        }

        #endregion

        public XmlNode SampleXml() {
            throw new NotImplementedException();
        }

    }

    public class removeStringFromTemplate : umbraco.interfaces.IPackageAction {
        #region IPackageAction Members

        public bool Execute(string packageName, XmlNode xmlData) {
            addStringToHtmlElement ast = new addStringToHtmlElement();
            return ast.Undo(packageName, xmlData);
        }

        public string Alias() {
            return "removeStringFromHtmlElement";
        }

        public bool Undo(string packageName, XmlNode xmlData) {
            return true;
        }

        public XmlNode SampleXml() {
            throw new NotImplementedException();
        }

        #endregion
    }


    public class helper {
        //Helper method to replace umbraco tags that breaks the xml format..
        public static string parseToValidXml(template.Template templateObj, ref bool hasAspNetContentBeginning, string template, bool toValid) {
            string retVal = template;
            if (toValid) {
                // test for asp:content as the first part of the design
                if (retVal.StartsWith("<asp:content", StringComparison.OrdinalIgnoreCase)) {
                    hasAspNetContentBeginning = true;
                    retVal = retVal.Substring(retVal.IndexOf(">") + 1);
                    retVal = retVal.Substring(0, retVal.Length - 14);
                }
                //shorten empty macro tags.. 
                retVal = retVal.Replace("></umbraco:macro>", " />");
                retVal = retVal.Replace("></umbraco:Macro>", " />");

                retVal = retVal.Replace("<umbraco:", "<umbraco__");
                retVal = retVal.Replace("</umbraco:", "</umbraco__");
                retVal = retVal.Replace("<asp:", "<asp__");
                retVal = retVal.Replace("</asp:", "</asp__");

                retVal = retVal.Replace("?UMBRACO_GETITEM", "UMBRACO_GETITEM");
                retVal = retVal.Replace("?UMBRACO_TEMPLATE_LOAD_CHILD", "UMBRACO_TEMPLATE_LOAD_CHILD");
                retVal = retVal.Replace("?UMBRACO_MACRO", "UMBRACO_MACRO");
                retVal = retVal.Replace("?ASPNET_FORM", "ASPNET_FORM");
                retVal = retVal.Replace("?ASPNET_HEAD", "ASPNET_HEAD");


            } else {
                retVal = retVal.Replace("<umbraco__", "<umbraco:");
                retVal = retVal.Replace("</umbraco__", "</umbraco:");
                retVal = retVal.Replace("<asp__", "<asp:");
                retVal = retVal.Replace("</asp__", "</asp:");
                retVal = retVal.Replace("UMBRACO_GETITEM", "?UMBRACO_GETITEM");
                retVal = retVal.Replace("UMBRACO_TEMPLATE_LOAD_CHILD", "?UMBRACO_TEMPLATE_LOAD_CHILD");
                retVal = retVal.Replace("UMBRACO_MACRO", "?UMBRACO_MACRO");
                retVal = retVal.Replace("ASPNET_FORM", "?ASPNET_FORM");
                retVal = retVal.Replace("ASPNET_HEAD", "?ASPNET_HEAD");
                retVal = retVal.Replace("<root>", "");
                retVal = retVal.Replace("<root xmlns:asp=\"http://microsoft.com\">", "");
                retVal = retVal.Replace("</root>", "");

                // add asp content element
                if (hasAspNetContentBeginning) {
                    retVal = templateObj.GetMasterContentElement(templateObj.MasterTemplate) + retVal + "</asp:content>";
                }
            }

            return retVal;
        }

        public static XmlNode parseStringToXmlNode(string value) {
            XmlDocument doc = new XmlDocument();
            XmlNode node = xmlHelper.addTextNode(doc, "error", "");
            
            try {
                doc.LoadXml(value);
                return doc.SelectSingleNode(".");
            } catch {
                return node;
            }

            return node;
        }
    }
}
