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
    [Obsolete("Used the TreeControl control instead. This does however get used by the TreeService when requesting the tree init url.")]
    public partial class TreeInit : UmbracoEnsuredPage
    {

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            TreeParams = TreeRequestParams.FromQueryStrings().CreateTreeService();
            DataBind();
        }

        protected TreeService TreeParams { get; private set; } 

		
    }
}