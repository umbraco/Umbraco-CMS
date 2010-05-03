using System.Collections;
using umbraco.BasePages;

namespace umbraco.cms.presentation
{
	/// <summary>
	/// Summary description for cacheBrowser.
	/// </summary>
    public partial class cacheBrowser : UmbracoEnsuredPage
	{
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Cache removal checks
			if (Request.QueryString["clearByType"] != null)
				cms.businesslogic.cache.Cache.ClearCacheObjectTypes(Request.QueryString["clearByType"]);
			else if (Request.QueryString["clearByKey"] != null)
				cms.businesslogic.cache.Cache.ClearCacheItem(Request.QueryString["clearByKey"]);

			// Put user code to initialize the page here
			Hashtable ht = cms.businesslogic.cache.Cache.ReturnCacheItemsOrdred();
			foreach(string key in ht.Keys) 
			{
				Response.Write("<a href=\"?key=" + key + "\">" + key + "</a>: " + ((ArrayList) ht[key]).Count.ToString() + " (<a href=\"?clearByType=" + key + "\">Delete</a>)<br />");
				if (Request.QueryString["key"] == key)
					for (int i=0; i<((ArrayList) ht[key]).Count;i++) 
						Response.Write(" - " + ((ArrayList) ht[key])[i] + " (<a href=\"?clearByKey=" + ((ArrayList) ht[key])[i] + "\">Delete</a>)<br />");
			}
		}
		protected void Button1_Click(object sender, System.EventArgs e)
		{
			cms.businesslogic.cache.Cache.ClearAllCache();
		}
	}
}
