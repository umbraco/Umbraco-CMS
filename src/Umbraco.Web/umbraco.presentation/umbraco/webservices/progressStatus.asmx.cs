using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using Umbraco.Web.WebServices;

namespace presentation.umbraco.webservices
{
	/// <summary>
	/// Summary description for progressStatus.
	/// </summary>
	[WebService(Namespace="http://umbraco.org/webservices/")]
    public class progressStatus : UmbracoAuthorizedWebService
	{
		
		[WebMethod]
		public int GetStatus(string key)
		{
		    if (!AuthorizeRequest()) return 0;

			try 
			{
				return int.Parse(Application[key].ToString());
			} 
			catch 
			{
				return 0;
			}
		}

	}
}
