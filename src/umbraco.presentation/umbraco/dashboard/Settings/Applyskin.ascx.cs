using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.skinning;
using umbraco.cms.businesslogic.template;

namespace umbraco.presentation.umbraco.dashboard.Settings
{
    public partial class Applyskin : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            skinpicker.Items.Add("Choose...");
            foreach (Skin s in Skinning.GetAllSkins())
            {
                skinpicker.Items.Add( new ListItem(s.Name, s.Alias));
            }
        }

        protected void apply(object sender, EventArgs e)
        {
            if (skinpicker.SelectedIndex > 0)
            {
                Skin s = Skin.CreateFromAlias(skinpicker.SelectedValue);
                Skinning.ActivateAsCurrentSkin(s);
            }
        }

        protected void rollback(object sender, EventArgs e)
        {
            Template t = Template.GetByAlias("RunwayHomepage");
            Skinning.RollbackSkin(t.Id);
        }


    }
}