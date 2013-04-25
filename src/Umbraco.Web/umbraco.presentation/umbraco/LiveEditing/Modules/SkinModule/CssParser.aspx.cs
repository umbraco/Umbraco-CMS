using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.skinning;

namespace umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule
{
    public partial class CssParser : BasePages.UmbracoEnsuredPage
    {

        public CssParser()
        {
            //for skinning, you need to be a developer
            CurrentApp = DefaultApps.developer.ToString();
        }

        //will be used to parse the global variables in the embedded css of a skin manifest
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "text/plain";

            var skinAlias = Request["skinAlias"];

            if (!string.IsNullOrEmpty(skinAlias) && Skin.CreateFromAlias(skinAlias) != null)
            {
                var activeSkin = Skin.CreateFromAlias(skinAlias);

                if (activeSkin.Css != null)
                {

                    var varValues = new SortedList<string, string>();

                    foreach (var var in activeSkin.Css.Variables)
                    {
                        varValues.Add(var.Name, string.IsNullOrEmpty(Request[var.Name]) ? var.DefaultValue : Request[var.Name]);

                    }

                    Response.Write(ParseEmbeddedCss(activeSkin.Css.Content,varValues));
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