using System;
using System.Web;
using umbraco.BasePages;

namespace umbraco.cms.presentation.developer
{
    /// <summary>
    /// Summary description for viewCacheItem.
    /// </summary>
    public partial class viewCacheItem : UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Put user code to initialize the page here
            Panel1.Text = ui.Text("viewCacheItem");
            string cacheKey = Request.QueryString["key"];
            LabelCacheAlias.Text = cacheKey;
            object cacheItem = HttpRuntime.Cache[cacheKey];
            LabelCacheValue.Text = cacheItem != null ? cacheItem.ToString() : "Cache item isn't in cache anymore!";
        }

        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        #endregion
    }
}