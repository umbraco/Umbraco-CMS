using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.presentation.Trees;

namespace Umbraco.Web.UI.Umbraco
{
    [Obsolete("Used the TreeControl control instead. This does however get used by the TreeService when requesting the tree init url.")]
    public partial class TreeInit : Pages.UmbracoEnsuredPage
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