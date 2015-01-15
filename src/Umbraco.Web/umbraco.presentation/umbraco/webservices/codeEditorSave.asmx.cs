using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Xml;
using System.Xml.Xsl;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web.WebServices;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;
using System.Net;
using System.Collections;
using umbraco.NodeFactory;

namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Summary description for codeEditorSave
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class codeEditorSave : UmbracoAuthorizedWebService
    {
       
        [WebMethod]
        public string SaveCss(string fileName, string oldName, string fileContents, int fileID)
        {
            if (AuthorizeRequest(DefaultApps.settings.ToString()))
            {
                var stylesheet = Services.FileService.GetStylesheetByName(oldName.EnsureEndsWith(".css"));
                if (stylesheet == null) throw new InvalidOperationException("No stylesheet found with name " + oldName);

                stylesheet.Content = fileContents;
                if (fileName.InvariantEquals(oldName) == false)
                {
                    //it's changed which means we need to change the path
                    stylesheet.Path = stylesheet.Path.TrimEnd(oldName.EnsureEndsWith(".css")) + fileName.EnsureEndsWith(".css");
                }

                Services.FileService.SaveStylesheet(stylesheet, Security.CurrentUser.Id);

                return "true";
            }
            return "false";
        }

        [WebMethod]
        public string SaveXslt(string fileName, string oldName, string fileContents, bool ignoreDebugging)
        {
            if (AuthorizeRequest(DefaultApps.developer.ToString()))
            {

                // validate file
                IOHelper.ValidateEditPath(IOHelper.MapPath(SystemDirectories.Xslt + "/" + fileName),
                                          SystemDirectories.Xslt);
                // validate extension
                IOHelper.ValidateFileExtension(IOHelper.MapPath(SystemDirectories.Xslt + "/" + fileName),
                                               new List<string>() { "xsl", "xslt" });


                StreamWriter SW;
                string tempFileName = IOHelper.MapPath(SystemDirectories.Xslt + "/" + DateTime.Now.Ticks + "_temp.xslt");
                SW = File.CreateText(tempFileName);
                SW.Write(fileContents);
                SW.Close();

                // Test the xslt
                string errorMessage = "";

                if (!ignoreDebugging)
                {
                    try
                    {
                        // Check if there's any documents yet
                        string xpath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "/root/node" : "/root/*";
                        if (content.Instance.XmlContent.SelectNodes(xpath).Count > 0)
                        {
                            var macroXML = new XmlDocument();
                            macroXML.LoadXml("<macro/>");

                            var macroXSLT = new XslCompiledTransform();
                            var umbPage = new page(content.Instance.XmlContent.SelectSingleNode("//* [@parentID = -1]"));

                            var xslArgs = macro.AddMacroXsltExtensions();
                            var lib = new library(umbPage);
                            xslArgs.AddExtensionObject("urn:umbraco.library", lib);
                            HttpContext.Current.Trace.Write("umbracoMacro", "After adding extensions");

                            // Add the current node
                            xslArgs.AddParam("currentPage", "", library.GetXmlNodeById(umbPage.PageID.ToString()));

                            HttpContext.Current.Trace.Write("umbracoMacro", "Before performing transformation");

                            // Create reader and load XSL file
                            // We need to allow custom DTD's, useful for defining an ENTITY
                            var readerSettings = new XmlReaderSettings();
                            readerSettings.ProhibitDtd = false;
                            using (var xmlReader = XmlReader.Create(tempFileName, readerSettings))
                            {
                                var xslResolver = new XmlUrlResolver();
                                xslResolver.Credentials = CredentialCache.DefaultCredentials;
                                macroXSLT.Load(xmlReader, XsltSettings.TrustedXslt, xslResolver);
                                xmlReader.Close();
                                // Try to execute the transformation
                                var macroResult = new HtmlTextWriter(new StringWriter());
                                macroXSLT.Transform(macroXML, xslArgs, macroResult);
                                macroResult.Close();

                                File.Delete(tempFileName);
                            }
                        }
                        else
                        {
                            //errorMessage = ui.Text("developer", "xsltErrorNoNodesPublished");
                            File.Delete(tempFileName);
                            //base.speechBubble(speechBubbleIcon.info, ui.Text("errors", "xsltErrorHeader", base.getUser()), "Unable to validate xslt as no published content nodes exist.");
                        }
                    }
                    catch (Exception errorXslt)
                    {
                        File.Delete(tempFileName);

                        errorMessage = (errorXslt.InnerException ?? errorXslt).ToString();

                        // Full error message
                        errorMessage = errorMessage.Replace("\n", "<br/>\n");
                        //closeErrorMessage.Visible = true;

                        // Find error
                        var m = Regex.Matches(errorMessage, @"\d*[^,],\d[^\)]", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                        foreach (Match mm in m)
                        {
                            string[] errorLine = mm.Value.Split(',');

                            if (errorLine.Length > 0)
                            {
                                var theErrorLine = int.Parse(errorLine[0]);
                                var theErrorChar = int.Parse(errorLine[1]);

                                errorMessage = "Error in XSLT at line " + errorLine[0] + ", char " + errorLine[1] +
                                               "<br/>";
                                errorMessage += "<span style=\"font-family: courier; font-size: 11px;\">";

                                var xsltText = fileContents.Split("\n".ToCharArray());
                                for (var i = 0; i < xsltText.Length; i++)
                                {
                                    if (i >= theErrorLine - 3 && i <= theErrorLine + 1)
                                        if (i + 1 == theErrorLine)
                                        {
                                            errorMessage += "<b>" + (i + 1) + ": &gt;&gt;&gt;&nbsp;&nbsp;" +
                                                            Server.HtmlEncode(xsltText[i].Substring(0, theErrorChar));
                                            errorMessage +=
                                                "<span style=\"text-decoration: underline; border-bottom: 1px solid red\">" +
                                                Server.HtmlEncode(
                                                    xsltText[i].Substring(theErrorChar,
                                                                          xsltText[i].Length - theErrorChar)).
                                                    Trim() + "</span>";
                                            errorMessage += " &lt;&lt;&lt;</b><br/>";
                                        }
                                        else
                                            errorMessage += (i + 1) + ": &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" +
                                                            Server.HtmlEncode(xsltText[i]) + "<br/>";
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

                    if (savePath.StartsWith(IOHelper.MapPath(SystemDirectories.Xslt + "/")))
                    {
                        //deletes the old xslt file
                        if (fileName != oldName)
                        {

                            var p = IOHelper.MapPath(SystemDirectories.Xslt + "/" + oldName);
                            if (File.Exists(p))
                                File.Delete(p);
                        }

                        SW = File.CreateText(savePath);
                        SW.Write(fileContents);
                        SW.Close();
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
            return "false";
        }

        [WebMethod]
        public string SaveDLRScript(string fileName, string oldName, string fileContents, bool ignoreDebugging)
        {
            if (AuthorizeRequest(DefaultApps.developer.ToString()))
            {
                if (string.IsNullOrEmpty(fileName))
                    throw new ArgumentNullException("fileName");

                var allowedExtensions = new List<string>();
                foreach (var lang in MacroEngineFactory.GetSupportedUILanguages())
                {
                    if (!allowedExtensions.Contains(lang.Extension))
                        allowedExtensions.Add(lang.Extension);
                }


                // validate file
                IOHelper.ValidateEditPath(IOHelper.MapPath(SystemDirectories.MacroScripts + "/" + fileName),
                                          SystemDirectories.MacroScripts);
                // validate extension
                IOHelper.ValidateFileExtension(IOHelper.MapPath(SystemDirectories.MacroScripts + "/" + fileName),
                                               allowedExtensions);


                //As Files Can Be Stored In Sub Directories, So We Need To Get The Exeuction Directory Correct
                var lastOccurance = fileName.LastIndexOf('/') + 1;
                var directory = fileName.Substring(0, lastOccurance);
                var fileNameWithExt = fileName.Substring(lastOccurance);
                var tempFileName =
                    IOHelper.MapPath(SystemDirectories.MacroScripts + "/" + directory + DateTime.Now.Ticks + "_" +
                                     fileNameWithExt);

                using (var sw = new StreamWriter(tempFileName, false, Encoding.UTF8))
                {
                    sw.Write(fileContents);
                    sw.Close();    
                }

                var errorMessage = "";
                if (!ignoreDebugging)
                {
                    var root = Document.GetRootDocuments().FirstOrDefault();
                    if (root != null)
                    {
                        var args = new Hashtable();
                        var n = new Node(root.Id);
                        args.Add("currentPage", n);

                        try
                        {
                            var engine = MacroEngineFactory.GetByFilename(tempFileName);
                            var tempErrorMessage = "";
                            var xpath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "/root/node" : "/root/*";
                            if (
                                !engine.Validate(fileContents, tempFileName, Node.GetNodeByXpath(xpath),
                                                 out tempErrorMessage))
                                errorMessage = tempErrorMessage;
                        }
                        catch (Exception err)
                        {
                            errorMessage = err.ToString();
                        }
                    }
                }

                if (errorMessage == "")
                {
                    var savePath = IOHelper.MapPath(SystemDirectories.MacroScripts + "/" + fileName);

                    //deletes the file
                    if (fileName != oldName)
                    {
                        var p = IOHelper.MapPath(SystemDirectories.MacroScripts + "/" + oldName);
                        if (File.Exists(p))
                            File.Delete(p);
                    }

                    using (var sw = new StreamWriter(savePath, false, Encoding.UTF8))
                    {
                        sw.Write(fileContents);
                        sw.Close();
                    }
                    errorMessage = "true";

                    
                }

                File.Delete(tempFileName);


                return errorMessage.Replace("\n", "<br/>\n");
            }

            return "false";
        }

		//[WebMethod]
		//public string SavePartialView(string filename, string oldName, string contents)
		//{
		//	if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
		//	{
		//		var folderPath = SystemDirectories.MvcViews + "/Partials/";

		//		// validate file
		//		IOHelper.ValidateEditPath(IOHelper.MapPath(folderPath + filename), folderPath);
		//		// validate extension
		//		IOHelper.ValidateFileExtension(IOHelper.MapPath(folderPath + filename), new[] {"cshtml"}.ToList());


		//		var val = contents;
		//		string returnValue;
		//		var saveOldPath = oldName.StartsWith("~/") ? IOHelper.MapPath(oldName) : IOHelper.MapPath(folderPath + oldName);
		//		var savePath = filename.StartsWith("~/") ? IOHelper.MapPath(filename) : IOHelper.MapPath(folderPath + filename);

		//		//Directory check.. only allow files in script dir and below to be edited
		//		if (savePath.StartsWith(IOHelper.MapPath(folderPath)))
		//		{
		//			//deletes the old file
		//			if (savePath != saveOldPath)
		//			{
		//				if (File.Exists(saveOldPath))
		//					File.Delete(saveOldPath);
		//			}
		//			using (var sw = File.CreateText(savePath))
		//			{
		//				sw.Write(val);
		//			}
		//			returnValue = "true";
		//		}
		//		else
		//		{
		//			returnValue = "illegalPath";
		//		}

		//		return returnValue;
		//	}
		//	return "false";
		//}

        [WebMethod]
        public string SaveScript(string filename, string oldName, string contents)
        {
            if (AuthorizeRequest(DefaultApps.settings.ToString()))
            {

                // validate file
                IOHelper.ValidateEditPath(IOHelper.MapPath(SystemDirectories.Scripts + "/" + filename),
                                          SystemDirectories.Scripts);
                // validate extension
                IOHelper.ValidateFileExtension(IOHelper.MapPath(SystemDirectories.Scripts + "/" + filename),
                                               UmbracoConfig.For.UmbracoSettings().Content.ScriptFileTypes.ToList());


                var val = contents;
                string returnValue;
                try
                {
                    var saveOldPath = "";
                    saveOldPath = oldName.StartsWith("~/") 
                        ? IOHelper.MapPath(oldName) 
                        : IOHelper.MapPath(SystemDirectories.Scripts + "/" + oldName);

                    var savePath = "";
                    savePath = filename.StartsWith("~/") 
                        ? IOHelper.MapPath(filename) 
                        : IOHelper.MapPath(SystemDirectories.Scripts + "/" + filename);
                    
                    //Directory check.. only allow files in script dir and below to be edited
                    if (savePath.StartsWith(IOHelper.MapPath(SystemDirectories.Scripts + "/")) || savePath.StartsWith(IOHelper.MapPath(SystemDirectories.Masterpages + "/")))
                    {
                        //deletes the old file
                        if (savePath != saveOldPath)
                        {
                            if (File.Exists(saveOldPath))
                                File.Delete(saveOldPath);
                        }
                        
                        using (var sw = File.CreateText(savePath))
                        {
                            sw.Write(val);
                            sw.Close();
                        }
                       
                        returnValue = "true";
                    }
                    else
                    {
                        returnValue = "illegalPath";
                    }
                }
                catch
                {
                    returnValue = "false";
                }


                return returnValue;
            }
            return "false";
        }
        
		[Obsolete("This method has been superceded by the REST service /Umbraco/RestServices/SaveFile/SaveTemplate which is powered by the SaveFileController.")]
        [WebMethod]
        public string SaveTemplate(string templateName, string templateAlias, string templateContents, int templateID, int masterTemplateID)
        {
            if (AuthorizeRequest(DefaultApps.settings.ToString()))
            {
                var _template = new Template(templateID);
                string retVal = "false";

                if (_template != null)
                {
                    _template.Text = templateName;
                    _template.Alias = templateAlias;
                    _template.MasterTemplate = masterTemplateID;
                    _template.Design = templateContents;

                    _template.Save();

                    retVal = "true";                    
                }
                return retVal;
            }
            return "false";
        }

    }
}