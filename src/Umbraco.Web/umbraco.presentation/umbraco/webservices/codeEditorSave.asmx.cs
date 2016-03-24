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
        public string SaveXslt(string fileName, string oldName, string fileContents, bool ignoreDebugging)
        {
            if (AuthorizeRequest(Constants.Applications.Developer.ToString()))
            {
                IOHelper.EnsurePathExists(SystemDirectories.Xslt);

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
                        string xpath = "/root/*";
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
                            //errorMessage = Services.TextService.Localize("developer/xsltErrorNoNodesPublished");
                            File.Delete(tempFileName);
                            //base.speechBubble(speechBubbleIcon.info, Services.TextService.Localize("errors/xsltErrorHeader"), "Unable to validate xslt as no published content nodes exist.");
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
    }
}