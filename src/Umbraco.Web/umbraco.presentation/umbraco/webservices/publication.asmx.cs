using System;
using System.ComponentModel;
using System.Web.Services;
using System.Web.Script.Services;
using Umbraco.Web.WebServices;
using umbraco.BusinessLogic;
using umbraco.presentation.webservices;

namespace umbraco.webservices
{
	/// <summary>
	/// Summary description for publication.
	/// </summary>
	[WebService(Namespace="http://umbraco.org/webservices/")]
    [ScriptService]
    public class publication : UmbracoAuthorizedWebService
	{
		
		[WebMethod]
        [ScriptMethod]
		public int GetPublicationStatus(string key)
		{
		    if (!AuthorizeRequest(DefaultApps.content.ToString()))
		        return 0;

			try 
			{
				return int.Parse(Application["publishDone" + key].ToString());
			} 
			catch 
			{
				return 0;
			}
		}

        [WebMethod]
        [ScriptMethod]
        public int GetPublicationStatusMax(string key)
        {
            if (!AuthorizeRequest(DefaultApps.content.ToString()))
                return 0;

            try
            {
                return int.Parse(Application["publishTotal" + key].ToString());
            }
            catch
            {
                return 0;
            }
        }

        [WebMethod]
        [ScriptMethod]
        public int GetPublicationStatusMaxAll(string key)
        {
            if (!AuthorizeRequest(DefaultApps.content.ToString()))
		        return 0;

            try
            {
                return int.Parse(Application["publishTotalAll" + key].ToString());
            }
            catch
            {
                return 0;
            }
        }

        [Obsolete("This doesn't do anything and will be removed in future versions")]
		[WebMethod]
		public void HandleReleaseAndExpireDates(Guid PublishingServiceKey) 
		{
		}

        [Obsolete("This doesn't do anything and will be removed in future versions")]
        [WebMethod]
        public void SaveXmlCacheToDisk()
        {
            if (!AuthorizeRequest(DefaultApps.content.ToString()))
                return;
        }

		
	}
}
