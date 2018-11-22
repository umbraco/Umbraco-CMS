using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Umbraco.Core.Configuration;
using umbraco.cms.businesslogic.propertytype;
using Umbraco.Core;

namespace umbraco.developer
{
	/// <summary>
	/// Summary description for xsltInsertValueOf.
	/// </summary>
    [WebformsPageTreeAuthorize(Constants.Trees.Xslt)]
	public partial class xsltInsertValueOf : BasePages.UmbracoEnsuredPage
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
            foreach (PropertyType pt in PropertyType.GetAll().Where(x => exclude.Contains(x.Alias) == false))
		    {
                if (!existingGenProps.Contains("," + pt.Alias + ","))
                {
                    if(UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema)
                        preValuesSource.Add(string.Format("data [@alias = '{0}']", pt.Alias));
                    else
                        preValuesSource.Add(pt.Alias);

                    existingGenProps += pt.Alias + ",";
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
