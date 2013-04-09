using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Web.Script.Services;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.presentation.webservices;

namespace umbraco.webservices
{
	/// <summary>
	/// Summary description for templates.
	/// </summary>
	[WebService(Namespace="http://umbraco.org/webservices/")]
    [ScriptService]
	public class templates : WebService
	{
		
		[WebMethod]
		public XmlNode GetTemplates(string Login, string Password)
		{
		    if (BusinessLogic.User.validateCredentials(Login, Password)) 
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml("<templates/>");
				foreach (cms.businesslogic.template.Template t in cms.businesslogic.template.Template.GetAllAsList()) 
				{
					var tt = xmlDoc.CreateElement("template");
					tt.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "id", t.Id.ToString()));
                    tt.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "name", t.Text));
					xmlDoc.DocumentElement.AppendChild(tt);
				}
				return xmlDoc.DocumentElement;
			}
		    return null;
		}

	    [WebMethod]
		public XmlNode GetTemplate(int Id, string Login, string Password)
		{
		    if (BusinessLogic.User.validateCredentials(Login, Password)) 
			{
				var t = new cms.businesslogic.template.Template(Id);
				var xmlDoc = new XmlDocument();
				var tXml = xmlDoc.CreateElement("template");
                tXml.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "id", t.Id.ToString()));
                tXml.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "master", t.MasterTemplate.ToString()));
                tXml.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "name", t.Text));
				tXml.AppendChild(XmlHelper.AddCDataNode(xmlDoc, "design", t.Design));
				return tXml;
			}
		    return null;
		}

	    [WebMethod]
		public bool UpdateTemplate(int Id, int Master, string Design, string Login, string Password)
		{
		    if (BusinessLogic.User.validateCredentials(Login, Password)) 
			{
                try
                {
                    var t = new cms.businesslogic.template.Template(Id)
                        {
                            MasterTemplate = Master,
                            Design = Design
                        };
                    //ensure events are raised
                    t.Save();
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }			    
			}
		    return false;
		}

	    [WebMethod]
        [ScriptMethod]
        public string GetCodeSnippet(object templateId)
        {
            legacyAjaxCalls.Authorize();
            
	        var templateFile = 
                System.IO.File.OpenText(IOHelper.MapPath(SystemDirectories.Umbraco + "/scripting/templates/cshtml/" + templateId));
            var content = templateFile.ReadToEnd();
            templateFile.Close();

            return content;
        }
		
	}
}
