using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.skinning;

namespace umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule
{
    public partial class CssParser : System.Web.UI.Page
    {
        //will be used to parse the global variables in the embedded css of a skin manifest
        protected void Page_Load(object sender, EventArgs e)
        {
            string skinAlias = Request["skinAlias"];

            if (!string.IsNullOrEmpty(skinAlias) && Skin.CreateFromAlias(skinAlias) != null)
            {
                Skin ActiveSkin = Skin.CreateFromAlias(skinAlias);

                if (ActiveSkin.Css != null)
                {

                    SortedList<string, string> varValues = new SortedList<string, string>();

                    foreach (CssVariable var in ActiveSkin.Css.Variables)
                    {
                        varValues.Add(var.Name, string.IsNullOrEmpty(Request[var.Name]) ? var.DefaultValue : Request[var.Name]);

                    }

                    Response.Write(ParseEmbeddedCss(ActiveSkin.Css.Content,varValues);
                }

            }
        }

        private string ParseEmbeddedCss(string content, SortedList<string, string> varValues)
        {
            foreach (var var in varValues)
            {
                content = content.Replace("@" + var.Key, var.Value);
            }

            return content;
        }
    }
}