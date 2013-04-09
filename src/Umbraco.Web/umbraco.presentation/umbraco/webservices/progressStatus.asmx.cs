using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;

namespace presentation.umbraco.webservices
{
	/// <summary>
	/// Summary description for progressStatus.
	/// </summary>
	[WebService(Namespace="http://umbraco.org/webservices/")]
	public class progressStatus : System.Web.Services.WebService
	{
		
		[WebMethod]
		public int GetStatus(string key) 
		{
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
