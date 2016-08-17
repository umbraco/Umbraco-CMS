using System.Globalization;
using System.Web.Services;
using System.Xml;
using Umbraco.Core;
using Umbraco.Web.WebServices;
using umbraco.BusinessLogic;
using umbraco.presentation.webservices;
using Umbraco.Core.Xml;

namespace umbraco.webservices
{
	/// <summary>
	/// Summary description for Developer.
	/// </summary>
	[WebService(Namespace="http://umbraco.org/webservices/")]
    public class Developer : UmbracoAuthorizedWebService
	{

		[WebMethod]
		public XmlNode GetMacros(string Login, string Password)
		{
		    if (ValidateCredentials(Login, Password) 
                && UserHasAppAccess(Constants.Applications.Developer.ToString(), Login))  
			{
				var xmlDoc = new XmlDocument();
				var macros = xmlDoc.CreateElement("macros");
				foreach (var m in cms.businesslogic.macro.Macro.GetAll()) 
				{
					var mXml = xmlDoc.CreateElement("macro");
					mXml.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "id", m.Id.ToString(CultureInfo.InvariantCulture)));
                    mXml.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "alias", m.Alias));
                    mXml.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "name", m.Name));
					macros.AppendChild(mXml);
				}
				return macros;
			}
		    return null;
		}

	    [WebMethod]
		public XmlNode GetMacro(int Id, string Login, string Password)
		{
		    if (ValidateCredentials(Login, Password)
                && UserHasAppAccess(Constants.Applications.Developer.ToString(), Login)) 
			{
				var xmlDoc = new XmlDocument();
				var macro = xmlDoc.CreateElement("macro");
				var m = new cms.businesslogic.macro.Macro(Id);
                macro.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "id", m.Id.ToString(CultureInfo.InvariantCulture)));
                macro.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "refreshRate", m.RefreshRate.ToString(CultureInfo.InvariantCulture)));
                macro.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "useInEditor", m.UseInEditor.ToString()));
                macro.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "alias", m.Alias));
                macro.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "name", m.Name));
                macro.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "type", m.Type));
                macro.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "xslt", m.Xslt));
				var properties = xmlDoc.CreateElement("properties");
				foreach (var mp in m.Properties) 
				{
					var pXml = xmlDoc.CreateElement("property");
                    pXml.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "alias", mp.Alias));
                    pXml.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "name", mp.Name));
				    pXml.Attributes.Append(XmlHelper.AddAttribute(xmlDoc, "public", true.ToString()));
					properties.AppendChild(pXml);
				}
				macro.AppendChild(properties);
				return macro;
			}
		    return null;
		}
	}
}
