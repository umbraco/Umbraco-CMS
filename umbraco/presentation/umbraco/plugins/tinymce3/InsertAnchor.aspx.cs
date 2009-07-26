using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.presentation.umbraco.plugins.tinymce3
{
	public partial class InsertAnchor : BasePages.UmbracoEnsuredPage
	{

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ClientLoader.DataBind();
		}

	}
}
