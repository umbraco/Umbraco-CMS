using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;

namespace umbraco.presentation.umbraco.dialogs
{
    public partial class insertMasterpageContent : BasePages.UmbracoEnsuredPage
    {
        public insertMasterpageContent()
        {
            CurrentApp = DefaultApps.settings.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //labels
            pp_placeholder.Text = ui.Text("placeHolderID");

            //Add a default Item
            var li = new ListItem("Choose ID...");
            li.Selected = true;
            dd_detectedAlias.Items.Add(li);

            var t = new cms.businesslogic.template.Template(int.Parse(Request["id"]));


            if (t.MasterTemplate > 0)
            {
                t = new cms.businesslogic.template.Template(t.MasterTemplate);

            }

            foreach (string cpId in t.contentPlaceholderIds())
            {
                dd_detectedAlias.Items.Add(cpId);
            }

            if (dd_detectedAlias.Items.Count == 1)
                dd_detectedAlias.Items.Add("ContentPlaceHolderDefault");

        }


    }
}
