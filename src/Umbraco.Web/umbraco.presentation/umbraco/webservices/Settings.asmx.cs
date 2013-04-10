using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Linq;
using System.Xml;
using Umbraco.Core;
using Umbraco.Web.WebServices;
using umbraco.BusinessLogic;

namespace umbraco.webservices
{

    public class Settings : UmbracoAuthorizedWebService
	{
		
		[WebMethod]
		public XmlNode GetTabs(string ContextID, int ContentTypeId)
		{
            if (!AuthorizeRequest(DefaultApps.settings.ToString()))
			{
				var xmlDoc = new XmlDocument();
				var tabs = xmlDoc.CreateElement("tabs");
                foreach (var t in new cms.businesslogic.ContentType(ContentTypeId).getVirtualTabs.ToList()) 
				{
					var mXml = xmlDoc.CreateElement("tab");
					mXml.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "id", t.Id.ToString()));
                    mXml.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "caption", t.Caption));
					tabs.AppendChild(mXml);
				}
				return tabs;
			}

		    return null;
		}
	}
}
