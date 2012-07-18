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
using umbraco.cms.presentation.Trees;

namespace umbraco.dialogs
{
    [Obsolete("Use the TreeControl instead. This does however get used by the TreeService when requesting the tree init url.")]
	public partial class treePicker : BasePages.UmbracoEnsuredPage
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
