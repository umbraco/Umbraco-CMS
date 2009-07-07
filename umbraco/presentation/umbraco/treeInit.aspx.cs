using System;
using System.Data;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco.cms.businesslogic;
using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.cms.presentation.Trees;
using System.Collections.Generic;
using System.Web;
using umbraco.BusinessLogic.Actions;
using System.Web.Services;

namespace umbraco.cms.presentation
{
    /// <summary>
    /// Summary description for TreeInit.
    /// </summary>
    public partial class TreeInit : UmbracoEnsuredPage
    {

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ClientLoader.DataBind();
		}
		
    }
}