using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using umbraco.cms.businesslogic.index;
using System.Threading;

namespace umbraco.cms.presentation
{
    public partial class reindex : BasePages.UmbracoEnsuredPage
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if ((Request["startIndexing"] != "" && 
                    System.Web.HttpContext.Current.Application["indexerReindexing"] != null && 
                    System.Web.HttpContext.Current.Application["indexerReindexing"].ToString() == "done") || 
                (System.Web.HttpContext.Current.Application["indexerReindexing"] == null ||
                String.IsNullOrEmpty(System.Web.HttpContext.Current.Application["indexerReindexing"].ToString()) && 
                !Page.IsPostBack))
            {
                System.Web.HttpContext.Current.Application["indexerReindexing"] = "true";
                HttpApplication ctx = (HttpApplication)HttpContext.Current.ApplicationInstance;
                Indexer.ResetIndexCounter(ctx, "Not calculated yet");
                ThreadPool.QueueUserWorkItem(delegate { Indexer.ReIndex(ctx); });
            }

            
            if (System.Web.HttpContext.Current.Application["indexerReindexing"] == null ||
                String.IsNullOrEmpty(System.Web.HttpContext.Current.Application["indexerReindexing"].ToString())) {
                    inProgress.Visible = false;
                    indexed.Visible = true;
                }
            else
            {
                inProgress.Visible = true;
                indexed.Visible = false;
                if (System.Web.HttpContext.Current.Application["umbIndexerCount"] != null)
                    reindexProgress.Text = System.Web.HttpContext.Current.Application["umbIndexerCount"].ToString();
                if (System.Web.HttpContext.Current.Application["umbIndexerTotal"] != null)
                    reindexTotal.Text = System.Web.HttpContext.Current.Application["umbIndexerTotal"].ToString();
                if (System.Web.HttpContext.Current.Application["umbIndexerInfo"] != null)
                    reindexCurrent.Text = System.Web.HttpContext.Current.Application["umbIndexerInfo"].ToString();
                if (System.Web.HttpContext.Current.Application["indexerReindexing"] != null &&
                    System.Web.HttpContext.Current.Application["indexerReindexing"].ToString() == "done")
                {
                    invoke.Visible = true;
                }

            }
        }
    }
}
