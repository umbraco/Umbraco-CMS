using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Security;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;

using System.Web.Script.Services;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using System.Web.UI;
using Umbraco.Core.IO;
using Umbraco.Web.WebServices;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.media;
using umbraco.BasePages;


namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Summary description for legacyAjaxCalls
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/webservices")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class legacyAjaxCalls : UmbracoAuthorizedWebService
    {
        [WebMethod]
        public bool ValidateUser(string username, string password)
        {

            if (ValidateCredentials(username, password))
            {
                var u = new BusinessLogic.User(username);
                BasePage.doLogin(u);
                return true;
            }
            else
            {
                return false;
            }

        }
        /// <summary>
        /// method to accept a string value for the node id. Used for tree's such as python
        /// and xslt since the file names are the node IDs
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="alias"></param>
        /// <param name="nodeType"></param>
        [WebMethod]
        [ScriptMethod]
        public void Delete(string nodeId, string alias, string nodeType)
        {

            AuthorizeRequest(true);

            //check which parameters to pass depending on the types passed in
            int intNodeID;
            if (int.TryParse(nodeId, out intNodeID) && nodeType != "member") // Fix for #26965 - numeric member login gets parsed as nodeId
                presentation.create.dialogHandler_temp.Delete(nodeType, intNodeID, alias);
            else
                presentation.create.dialogHandler_temp.Delete(nodeType, 0, nodeId);
        }
        
        /// <summary>
        /// Permanently deletes a document/media object.
        /// Used to remove an item from the recycle bin.
        /// </summary>
        /// <param name="nodeId"></param>
        [WebMethod]
        [ScriptMethod]
        public void DeleteContentPermanently(string nodeId, string nodeType)
        {
            int intNodeID;
            if (int.TryParse(nodeId, out intNodeID))
            {
                switch (nodeType)
                {
                    case "media":
                    case "mediaRecycleBin":
                        //ensure user has access to media
                        AuthorizeRequest(DefaultApps.media.ToString(), true);

                        new Media(intNodeID).delete(true);
                        break;
                    case "content":
                    case "contentRecycleBin":
                    default:
                        //ensure user has access to content
                        AuthorizeRequest(DefaultApps.content.ToString(), true);
                        
                        new Document(intNodeID).delete(true);
                        break;
                }                
            }
            else
            {
                throw new ArgumentException("The nodeId argument could not be parsed to an integer");
            }
        }

        [WebMethod]
        [ScriptMethod]
        public void DisableUser(int userId)
        {
            AuthorizeRequest(DefaultApps.users.ToString(), true);

            BusinessLogic.User.GetUser(userId).disable();
        }

        [WebMethod]
        [ScriptMethod]
        public string GetNodeName(int nodeId)
        {

            AuthorizeRequest(true);

            return new cms.businesslogic.CMSNode(nodeId).Text;
        }

        [WebMethod]
        [ScriptMethod]
        public string[] GetNodeBreadcrumbs(int nodeId)
        {

            AuthorizeRequest(true);

            var node = new cms.businesslogic.CMSNode(nodeId);
            var crumbs = new System.Collections.Generic.List<string>() { node.Text };
            while (node != null && node.Level > 1)
            {
                node = node.Parent;
                crumbs.Add(node.Text);
            }
            crumbs.Reverse();
            return crumbs.ToArray();
        }

        [WebMethod]
        [ScriptMethod]
        public string NiceUrl(int nodeId)
        {

            AuthorizeRequest(true);

            return library.NiceUrl(nodeId);
        }

        [WebMethod]
        [ScriptMethod]
        public string ProgressStatus(string Key)
        {
            AuthorizeRequest(true);

            return Application[helper.Request("key")].ToString();
        }

        [WebMethod]
        [ScriptMethod]
        public void RenewUmbracoSession()
        {
            AuthorizeRequest(true);

            BasePage.RenewLoginTimeout();

        }

        [WebMethod]
        [ScriptMethod]
        public int GetSecondsBeforeUserLogout()
        {
            //TODO: Change this to not throw an exception otherwise we end up with JS errors all the time when recompiling!!

            AuthorizeRequest(true);
            long timeout = BasePage.GetTimeout(true);
            DateTime timeoutDate = new DateTime(timeout);
            DateTime currentDate = DateTime.Now;
            
            return (int) timeoutDate.Subtract(currentDate).TotalSeconds;

        }

        [WebMethod]
        [ScriptMethod]
        public string TemplateMasterPageContentContainer(int templateId, int masterTemplateId)
        {
            AuthorizeRequest(DefaultApps.settings.ToString(), true);
            return new cms.businesslogic.template.Template(templateId).GetMasterContentElement(masterTemplateId);
        }

        [WebMethod]
        [ScriptMethod]
        public string SaveFile(string fileName, string fileAlias, string fileContents, string fileType, int fileID, int masterID, bool ignoreDebug)
        {
            switch (fileType)
            {
                case "xslt":
                    AuthorizeRequest(DefaultApps.developer.ToString(), true);
                    return SaveXslt(fileName, fileContents, ignoreDebug);
                case "python":
                    AuthorizeRequest(DefaultApps.developer.ToString(), true);
                    return "true";
                case "css":
                    AuthorizeRequest(DefaultApps.settings.ToString(), true);
                    return SaveCss(fileName, fileContents, fileID);
                case "script":
                    AuthorizeRequest(DefaultApps.settings.ToString(), true);
                    return SaveScript(fileName, fileContents);
                case "template":
                    AuthorizeRequest(DefaultApps.settings.ToString(), true);
                    return SaveTemplate(fileName, fileAlias, fileContents, fileID, masterID);
                default:
                    throw new ArgumentException(String.Format("Invalid fileType passed: '{0}'", fileType));
            }

        }

        public string Tidy(string textToTidy)
        {

            AuthorizeRequest(true);
            return library.Tidy(helper.Request("StringToTidy"), true);

        }

        private static string SaveCss(string fileName, string fileContents, int fileID)
        {
            string returnValue;
            var stylesheet = new StyleSheet(fileID) {Content = fileContents, Text = fileName};

	        try
	        {
		        stylesheet.saveCssToFile();
		        returnValue = "true";
	        }
	        catch (Exception ee)
	        {
		        throw new Exception("Couldn't save file", ee);
	        }

	        //this.speechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editStylesheetSaved", base.getUser()), "");
	        return returnValue;
        }

        private string SaveXslt(string fileName, string fileContents, bool ignoreDebugging)
        {	        
			var tempFileName = IOHelper.MapPath(SystemDirectories.Xslt + "/" + System.DateTime.Now.Ticks + "_temp.xslt");
            using (var sw = File.CreateText(tempFileName))
            {
				sw.Write(fileContents);
				sw.Close();    
            }
            
            // Test the xslt
            string errorMessage = "";
            if (!ignoreDebugging)
            {
                try
                {

                    // Check if there's any documents yet
                    if (content.Instance.XmlContent.SelectNodes("/root/node").Count > 0)
                    {
                        XmlDocument macroXML = new XmlDocument();
                        macroXML.LoadXml("<macro/>");

                        XslCompiledTransform macroXSLT = new XslCompiledTransform();
                        page umbPage = new page(content.Instance.XmlContent.SelectSingleNode("//node [@parentID = -1]"));

                        XsltArgumentList xslArgs;
                        xslArgs = macro.AddMacroXsltExtensions();
                        library lib = new library(umbPage);
                        xslArgs.AddExtensionObject("urn:umbraco.library", lib);
                        HttpContext.Current.Trace.Write("umbracoMacro", "After adding extensions");

                        // Add the current node
                        xslArgs.AddParam("currentPage", "", library.GetXmlNodeById(umbPage.PageID.ToString()));
                        HttpContext.Current.Trace.Write("umbracoMacro", "Before performing transformation");

                        // Create reader and load XSL file
                        // We need to allow custom DTD's, useful for defining an ENTITY
                        XmlReaderSettings readerSettings = new XmlReaderSettings();
                        readerSettings.ProhibitDtd = false;
                        using (XmlReader xmlReader = XmlReader.Create(tempFileName, readerSettings))
                        {
                            XmlUrlResolver xslResolver = new XmlUrlResolver();
                            xslResolver.Credentials = CredentialCache.DefaultCredentials;
                            macroXSLT.Load(xmlReader, XsltSettings.TrustedXslt, xslResolver);
                            xmlReader.Close();
                            // Try to execute the transformation
                            HtmlTextWriter macroResult = new HtmlTextWriter(new StringWriter());
                            macroXSLT.Transform(macroXML, xslArgs, macroResult);
                            macroResult.Close();
                        }
                    }
                    else
                    {
                        errorMessage = "stub";
                        //base.speechBubble(speechBubbleIcon.info, ui.Text("errors", "xsltErrorHeader", base.getUser()), "Unable to validate xslt as no published content nodes exist.");
                    }

                }
                catch (Exception errorXslt)
                {
                    //base.speechBubble(speechBubbleIcon.error, ui.Text("errors", "xsltErrorHeader", base.getUser()), ui.Text("errors", "xsltErrorText", base.getUser()));

                    //errorHolder.Visible = true;
                    //closeErrorMessage.Visible = true;
                    //errorHolder.Attributes.Add("style", "height: 250px; overflow: auto; border: 1px solid CCC; padding: 5px;");

                    errorMessage = (errorXslt.InnerException ?? errorXslt).ToString();

                    // Full error message
                    errorMessage = errorMessage.Replace("\n", "<br/>\n");
                    //closeErrorMessage.Visible = true;

                    string[] errorLine;
                    // Find error
                    MatchCollection m = Regex.Matches(errorMessage, @"\d*[^,],\d[^\)]", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                    foreach (Match mm in m)
                    {
                        errorLine = mm.Value.Split(',');

                        if (errorLine.Length > 0)
                        {
                            int theErrorLine = int.Parse(errorLine[0]);
                            int theErrorChar = int.Parse(errorLine[1]);

                            errorMessage = "Error in XSLT at line " + errorLine[0] + ", char " + errorLine[1] + "<br/>";
                            errorMessage += "<span style=\"font-family: courier; font-size: 11px;\">";

                            string[] xsltText = fileContents.Split("\n".ToCharArray());
                            for (int i = 0; i < xsltText.Length; i++)
                            {
                                if (i >= theErrorLine - 3 && i <= theErrorLine + 1)
                                    if (i + 1 == theErrorLine)
                                    {
                                        errorMessage += "<b>" + (i + 1) + ": &gt;&gt;&gt;&nbsp;&nbsp;" + Server.HtmlEncode(xsltText[i].Substring(0, theErrorChar));
                                        errorMessage += "<span style=\"text-decoration: underline; border-bottom: 1px solid red\">" + Server.HtmlEncode(xsltText[i].Substring(theErrorChar, xsltText[i].Length - theErrorChar)).Trim() + "</span>";
                                        errorMessage += " &lt;&lt;&lt;</b><br/>";
                                    }
                                    else
                                        errorMessage += (i + 1) + ": &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + Server.HtmlEncode(xsltText[i]) + "<br/>";
                            }
                            errorMessage += "</span>";

                        }
                    }
                }

            }


            if (errorMessage == "" && fileName.ToLower().EndsWith(".xslt"))
            {
                //Hardcoded security-check... only allow saving files in xslt directory... 
                var savePath = IOHelper.MapPath(SystemDirectories.Xslt + "/" + fileName);

                if (savePath.StartsWith(IOHelper.MapPath(SystemDirectories.Xslt)))
                {
					using (var sw = File.CreateText(savePath))
	                {
						sw.Write(fileContents);
						sw.Close();
	                }
                    errorMessage = "true";
                }
                else
                {
                    errorMessage = "Illegal path";
                }
            }

            File.Delete(tempFileName);


            return errorMessage;
        }
		
        private static string SaveScript(string filename, string contents)
        {
            var val = contents;
            string returnValue;
            try
            {
                var savePath = IOHelper.MapPath(SystemDirectories.Scripts + "/" + filename);

                //Directory check.. only allow files in script dir and below to be edited
                if (savePath.StartsWith(IOHelper.MapPath(SystemDirectories.Scripts + "/")))
                {
                    StreamWriter SW;
                    SW = File.CreateText(IOHelper.MapPath(SystemDirectories.Scripts + "/" + filename));
                    SW.Write(val);
                    SW.Close();
                    returnValue = "true";
                }
                else
                {
                    throw new ArgumentException("Couldnt save to file - Illegal path");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format("Couldnt save to file '{0}'", filename), ex);
            }


            return returnValue;
        }

        private static string SaveTemplate(string templateName, string templateAlias, string templateContents, int templateID, int masterTemplateID)
        {
            var tp = new cms.businesslogic.template.Template(templateID);
            var retVal = "false";

	        tp.Text = templateName;
	        tp.Alias = templateAlias;
	        tp.MasterTemplate = masterTemplateID;
	        tp.Design = templateContents;

	        retVal = "true";

	        // Clear cache in rutime
	        if (UmbracoSettings.UseDistributedCalls)
		        cache.dispatcher.Refresh(
			        new Guid("dd12b6a0-14b9-46e8-8800-c154f74047c8"),
			        tp.Id);
	        else
		        template.ClearCachedTemplate(tp.Id);


	        return retVal;
        }

        [Obsolete("You should use the AuthorizeRequest methods on the base class of UmbracoAuthorizedWebService and ensure you inherit from that class for umbraco asmx web services")]
        public static void Authorize()
        {
            // check for secure connection
            if (GlobalSettings.UseSSL && !HttpContext.Current.Request.IsSecureConnection)
                throw new UserAuthorizationException("This installation requires a secure connection (via SSL). Please update the URL to include https://");

            if (!BasePages.BasePage.ValidateUserContextID(BasePages.BasePage.umbracoUserContextID))
                throw new Exception("Client authorization failed. User is not logged in");

        }
    }
}
