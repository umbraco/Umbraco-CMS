using System;
using System.Web;
using Umbraco.Web;
using umbraco.BasePages;

namespace umbraco.cms.presentation.developer
{
    /// <summary>
    /// Summary description for viewCacheItem.
    /// </summary>
    public partial class viewCacheItem : UmbracoEnsuredPage
    {
        public viewCacheItem()
        {
            CurrentApp = BusinessLogic.DefaultApps.developer.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Panel1.Text = ui.Text("viewCacheItem");
            var cacheKey = Request.CleanForXss("key");
            LabelCacheAlias.Text = cacheKey;
            var cacheItem = ApplicationContext.ApplicationCache.GetCacheItem<object>(cacheKey);            
            LabelCacheValue.Text = cacheItem != null ? cacheItem.ToString() : "Cache item isn't in cache anymore!";
        }

    }
}