using Umbraco.Core.Services;
using System;
using System.Linq;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace umbraco.presentation.umbraco.dialogs
{
    public partial class insertMasterpageContent : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {
        public insertMasterpageContent()
        {
            CurrentApp = Constants.Applications.Settings.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //labels
            pp_placeholder.Text = Services.TextService.Localize("placeHolderID");

            //Add a default Item
            var li = new ListItem("Choose ID...");
            li.Selected = true;
            dd_detectedAlias.Items.Add(li);

            //var t = new cms.businesslogic.template.Template(int.Parse(Request["id"]));
            var t = Services.FileService.GetTemplate(int.Parse(Request["id"]));


            //if (t.MasterTemplate > 0)
            if (string.IsNullOrWhiteSpace(t.MasterTemplateAlias) != true)
            {
                //t = new cms.businesslogic.template.Template(t.MasterTemplate);
                t = Services.FileService.GetTemplate(t.MasterTemplateAlias);

            }

            //foreach (string cpId in t.contentPlaceholderIds())
            foreach (string cpId in MasterPageHelper.GetContentPlaceholderIds(t))
            {
                dd_detectedAlias.Items.Add(cpId);
            }

            if (dd_detectedAlias.Items.Count == 1)
                dd_detectedAlias.Items.Add("ContentPlaceHolderDefault");

        }


    }
}
