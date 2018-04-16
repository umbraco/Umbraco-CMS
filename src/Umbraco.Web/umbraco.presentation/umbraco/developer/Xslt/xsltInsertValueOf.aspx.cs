using System;
using System.Collections;
using System.Linq;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Web.UI.Pages;

namespace umbraco.developer
{
    /// <summary>
    /// Summary description for xsltInsertValueOf.
    /// </summary>
    [WebformsPageTreeAuthorize(Constants.Trees.Xslt)]
    public partial class xsltInsertValueOf : UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            ArrayList preValuesSource = new ArrayList();

            // Attributes
            string[] attributes = {"@id", "@parentID", "@level", "@writerID", "@nodeType", "@template", "@sortOrder", "@createDate", "@creatorName", "@updateDate", "@nodeName", "@urlName", "@writerName", "@nodeTypeAlias", "@path"};
            foreach (string att in attributes)
                preValuesSource.Add(att);

            // generic properties
            string existingGenProps = ",";
            var exclude = Constants.Conventions.Member.GetStandardPropertyTypeStubs().Select(x => x.Key).ToArray();

            var propertyTypes = Services.ContentTypeService.GetAllPropertyTypeAliases();

            foreach (var ptAlias in propertyTypes.Where(x => exclude.Contains(x) == false))
            {
                if (!existingGenProps.Contains("," + ptAlias + ","))
                {
                    preValuesSource.Add(ptAlias);


                    existingGenProps += ptAlias + ",";
                }
            }


            preValuesSource.Sort();
            preValues.DataSource = preValuesSource;
            preValues.DataBind();
            preValues.Items.Insert(0, new ListItem("Prevalues...", ""));

            preValues.Attributes.Add("onChange", "if (this.value != '') document.getElementById('" + valueOf.ClientID + "').value = this.value");

            if(!String.IsNullOrEmpty(Request.QueryString["value"]))
                valueOf.Text = Request.QueryString["value"];
        }

    }
}
