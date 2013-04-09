using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;

namespace umbraco.presentation.umbraco.plugins.tinymce3
{
    public partial class insertChar : BasePages.UmbracoEnsuredPage
	{
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //this could be used for media or content so we need to at least validate that the user has access to one or the other
            if (!ValidateUserApp(DefaultApps.content.ToString()) && !ValidateUserApp(DefaultApps.media.ToString()))
                throw new UserAuthorizationException("The current user doesn't have access to the section/app");
        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ClientLoader.DataBind();
		}
	}
}
