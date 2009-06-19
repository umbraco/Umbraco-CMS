using System;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace umbraco
{
	/// <summary>
	/// Summary description for formHandler.
	/// </summary>
	public class formHandler
	{
		private string _alias = "";
		private string _fhAssembly = "";
		private string _fhType = "";
		private XmlNode _formHandler;
		public formHandler(string alias)
		{
			_alias = alias;
            XmlDocument formHandlers = new XmlDocument();
			formHandlers.Load(System.Web.HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/../config/formHandlers.config"));
			_formHandler = formHandlers.SelectSingleNode("//formHandler [@alias='" + alias + "']");
			if (_formHandler != null) 
			{
				_fhAssembly = _formHandler.Attributes.GetNamedItem("assembly").Value;
				_fhType = _formHandler.Attributes.GetNamedItem("type").Value;

			}
		}

		public int Execute() 
		{
			int redirectID = -1;
			try 
			{
				// Reflect to execute and check whether the type is umbraco.main.IFormhandler
				Assembly assembly = Assembly.LoadFrom(System.Web.HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/../bin/"+_fhAssembly+".dll"));
				Type type = assembly.GetType(_fhAssembly+"."+_fhType);
				interfaces.IFormhandler typeInstance = Activator.CreateInstance(type) as interfaces.IFormhandler;
				if (typeInstance != null) 
				{
					typeInstance.Execute(_formHandler);
					if (typeInstance.redirectID > 0) 
					{
						redirectID = typeInstance.redirectID;
					}
					System.Web.HttpContext.Current.Trace.Write("formHandler", "Formhandler '" + _alias + "' executed with redirectID = " + redirectID);
				} 
				else
					System.Web.HttpContext.Current.Trace.Warn("formhandler", "Formhandler '" + _alias + "' doesn't implements interface umbraco.interfaces.IFormhandler");
			} 
			catch (Exception e) 
			{ 
				System.Web.HttpContext.Current.Trace.Warn("formhandler", "Error implementing formhandler '" + _alias + "'", e);
			}

			return redirectID;
		}
	}
}
