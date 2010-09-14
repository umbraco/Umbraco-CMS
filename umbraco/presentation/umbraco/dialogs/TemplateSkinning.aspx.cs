using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.skinning;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic;

namespace umbraco.presentation.umbraco.dialogs
{
    public partial class TemplateSkinning : System.Web.UI.Page
    {
        private int templateID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            templateID = int.Parse(Request["id"]);
            Template t = new Template(templateID);

            if (Skinning.StarterKitGuid(templateID).HasValue)
            {
                p_apply.Visible = true;

                string currentSkin = Skinning.GetCurrentSkinAlias(templateID);
                int templateRoot = FindTemplateRoot((CMSNode)t);

                dd_skins.Items.Add("Choose...");
                foreach (KeyValuePair<string,string> kvp in Skinning.AllowedSkins(templateRoot))
                {
                    ListItem li = new ListItem(kvp.Value, kvp.Key);
                    if (kvp.Key == currentSkin)
                        li.Selected = true;

                    dd_skins.Items.Add(li);
                }

                if (!string.IsNullOrEmpty(Skinning.GetCurrentSkinAlias(templateID)))
                {
                    ph_rollback.Visible = true;
                }
            }
        }

        private int FindTemplateRoot(CMSNode t)
        {
            if (t.ParentId < 0)
                return t.Id;
            else
                return FindTemplateRoot(t.Parent);
        }

        protected void openRepo(object sender, EventArgs e) { }
        protected void apply(object sender, EventArgs e) {

            if (dd_skins.SelectedIndex > 0)
            {
                Skinn

            }

        }
        protected void rollback(object sender, EventArgs e) { }
    }
}