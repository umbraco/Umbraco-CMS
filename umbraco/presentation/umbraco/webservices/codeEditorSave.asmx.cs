using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Xml;
using System.Xml.Xsl;
using umbraco.BasePages;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.cache;
using System.Net;
using umbraco.IO;
using System.Collections;
using umbraco.NodeFactory;
using umbraco.scripting;

namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Summary description for codeEditorSave
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class codeEditorSave : WebService
    {
        [WebMethod]
        public string Save(string fileName, string fileAlias, string fileContents, string fileType, int fileID,
                           int masterID, bool ignoreDebug)
        {
            return "Not implemented";
        }

        #region methods from savefile.aspx

        [WebMethod]
        public string SaveCss(string fileName, string oldName, string fileContents, int fileID)
        {
            if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
            {
                string returnValue = "false";
                StyleSheet stylesheet = new StyleSheet(fileID);

                if (stylesheet != null)
                {
                    stylesheet.Content = fileContents;
                    stylesheet.Text = fileName;
                    try
                    {
                        stylesheet.saveCssToFile();
                        stylesheet.Save();
                        returnValue = "true";


                        //deletes the old css file if the name was changed... 
                        if (fileName != oldName) {
                            string p = IOHelper.MapPath(SystemDirectories.Css + "/" + oldName + ".css");
                            if (System.IO.File.Exists( p ))
                                System.IO.File.Delete(p);
                        }

                    }
                    catch (Exception ex)
                    {
                        return ex.ToString();
                    }

                    //this.speechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editStylesheetSaved", base.getUser()), "");
                }
                return returnValue;
            }
            return "false";
        }

        [WebMethod]
        public string SaveXslt(string fileName, string oldName, string fileContents, bool ignoreDebugging)
        {
            if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
            {
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
                        string xpath = UmbracoSettings.UseLegacyXmlSchema ? "/root/node" : "/root/*";
                        if (content.Instance.XmlContent.SelectNodes(xpath).Count > 0)
                        {
                            XmlDocument macroXML = new XmlDocument();
                            macroXML.LoadXml("<macro/>");

                            XslCompiledTransform macroXSLT = new XslCompiledTransform();
                            page umbPage = new page(content.Instance.XmlContent.SelectSingleNode("//* [@parentID = -1]"));

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

                                File.Delete(tempFileName);
                            }
                        }
                        else
                        {
                            errorMessage = ui.Text("developer", "xsltErrorNoNodesPublished");
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

                                errorMessage = "Error in XSLT at line " + errorLine[0] + ", char " + errorLine[1] +
                                               "<br/>";
                                errorMessage += "<span style=\"font-family: courier; font-size: 11px;\">";

                                string[] xsltText = fileContents.Split("\n".ToCharArray());
                                for (int i = 0; i < xsltText.Length; i++)
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
                    string savePath = IOHelper.MapPath(SystemDirectories.Xslt + "/" + fileName);

                    if (savePath.StartsWith(IOHelper.MapPath(SystemDirectories.Xslt + "/")))
                    {
                        SW = File.CreateText(savePath);
                        SW.Write(fileContents);
                        SW.Close();
                        errorMessage = "true";

                        //deletes the old xslt file
                        if (fileName != oldName) {
                            string p = IOHelper.MapPath(SystemDirectories.Xslt + "/" + oldName);
                            if (System.IO.File.Exists(p))
                                System.IO.File.Delete(p);
                        }
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
            StreamWriter SW;
            string tempFileName = IOHelper.MapPath(SystemDirectories.Python + "/" + System.DateTime.Now.Ticks.ToString() + "_" + fileName);
            
    
			//SW = File.CreateText(tempFileName);
            SW = new System.IO.StreamWriter(tempFileName, false, new System.Text.UTF8Encoding(false));
            SW.Write(fileContents);
			SW.Close();
                
				string errorMessage = "";

                if (!ignoreDebugging)
                {
                    Hashtable args = new Hashtable();

                    Node n = new Node(Document.GetRootDocuments()[0].Id);
                    args.Add("currentPage", n);

                    try
                    {
                        // TODO: Hook the new MacroEngine
//                        MacroScript.ExecuteFile(tempFileName, args);
                    }
                    catch (Exception err)
                    {
                        errorMessage = err.ToString();
                        errorMessage = errorMessage.Replace("\n", "<br/>\n");
                    }
                }

                if (errorMessage == "")
                {
                    //Hardcoded security-check... only allow saving files in xslt directory... 
                    string savePath = IOHelper.MapPath(SystemDirectories.Python + "/" + fileName);

                    if (savePath.StartsWith(IOHelper.MapPath(SystemDirectories.Python + "/")))
                    {
                        SW = File.CreateText(savePath);
                        SW.Write(fileContents);
                        SW.Close();
                        errorMessage = "true";

                        //deletes the old xslt file
                        if (fileName != oldName)
                        {
                            string p = IOHelper.MapPath(SystemDirectories.Python + "/" + oldName);
                            if (System.IO.File.Exists(p))
                                System.IO.File.Delete(p);
                        }
                    }
                    else
                    {
                        errorMessage = "Illegal path";
                    }
                }

                File.Delete(tempFileName);
                

                return errorMessage;
        }

        [WebMethod]
        public string SaveScript(string filename, string oldName, string contents)
        {
            if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
            {
                string val = contents;
                string returnValue = "false";
                try
                {
                    string saveOldPath = "";
                    if (oldName.StartsWith("~/"))
                        saveOldPath = IOHelper.MapPath(oldName);
                    else
                        saveOldPath = IOHelper.MapPath(SystemDirectories.Scripts + "/" + oldName);

                    string savePath = "";
                    if (filename.StartsWith("~/"))
                        savePath = IOHelper.MapPath(filename);
                    else
                        savePath = IOHelper.MapPath(SystemDirectories.Scripts + "/" + filename);


                    //Directory check.. only allow files in script dir and below to be edited
                    if (savePath.StartsWith(IOHelper.MapPath(SystemDirectories.Scripts + "/")) || savePath.StartsWith(IOHelper.MapPath(SystemDirectories.Masterpages + "/")))
                    {
                        StreamWriter SW;
                        SW = File.CreateText(savePath);
                        SW.Write(val);
                        SW.Close();

                        //deletes the old file
                        if (savePath != saveOldPath)
                        {
                            if (System.IO.File.Exists(saveOldPath))
                                System.IO.File.Delete(saveOldPath);
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

        [WebMethod]
        public string SaveTemplate(string templateName, string templateAlias, string templateContents, int templateID, int masterTemplateID)
        {
            if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
            {
                Template _template = new Template(templateID);
                string retVal = "false";

                if (_template != null)
                {
                    _template.Text = templateName;
                    _template.Alias = templateAlias;
                    _template.MasterTemplate = masterTemplateID;
                    _template.Design = templateContents;
                    _template.Save();

                    retVal = "true";

                    // Clear cache in rutime
                    if (UmbracoSettings.UseDistributedCalls)
                        dispatcher.Refresh(new Guid("dd12b6a0-14b9-46e8-8800-c154f74047c8"),_template.Id);
                    else
                        template.ClearCachedTemplate(_template.Id);
                }
                else
                    return "false";


                return retVal;
            }
            return "false";
        }

        #endregion
    }
}