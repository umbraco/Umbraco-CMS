using System;
using System.ComponentModel;
using System.Web.Services;
using System.Web.Script.Services;
using umbraco.presentation.webservices;

namespace umbraco.webservices
{
	/// <summary>
	/// Summary description for publication.
	/// </summary>
	[WebService(Namespace="http://umbraco.org/webservices/")]
    [ScriptService]
	public class publication : WebService
	{
		
		[WebMethod]
        [ScriptMethod]
		public int GetPublicationStatus(string key) 
		{
            legacyAjaxCalls.Authorize();

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
            legacyAjaxCalls.Authorize();

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
            try
            {
                return int.Parse(Application["publishTotalAll" + key].ToString());
            }
            catch
            {
                return 0;
            }
        }

		[WebMethod]
		public void HandleReleaseAndExpireDates(Guid PublishingServiceKey) 
		{
		}

        [WebMethod]
        public void SaveXmlCacheToDisk()
        {
            legacyAjaxCalls.Authorize();

            content.Instance.PersistXmlToFile();
        }

	}
}
