using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using umbraco.DataLayer;

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for umbracoField.
	/// </summary>
	public partial class umbracoField : BasePages.UmbracoEnsuredPage
	{
        string[] preValuesSource = { "@createDate", "@creatorName", "@level", "@nodeType", "@nodeTypeAlias", "@pageID", "@pageName", "@parentID", "@path", "@template", "@updateDate", "@writerID", "@writerName" };
        bool m_IsDictionaryMode = false;

	    public umbracoField()
	    {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();
	    }

		protected void Page_Load(object sender, System.EventArgs e)
		{

            //set labels on properties...
            pp_insertAltField.Text = ui.Text("templateEditor", "alternativeField");
            pp_insertAltText.Text = ui.Text("templateEditor", "alternativeText");
            pp_recursive.Text = ui.Text("templateEditor", "recursive");
            pp_insertBefore.Text = ui.Text("templateEditor", "preContent");
            pp_insertAfter.Text = ui.Text("templateEditor", "postContent");
            
            pp_FormatAsDate.Text = ui.Text("templateEditor", "formatAsDate");
            pp_casing.Text = ui.Text("templateEditor", "casing");
            pp_encode.Text = ui.Text("templateEditor", "encoding");
            pp_convertLineBreaks.Text = ui.Text("templateEditor", "convertLineBreaks");
            pp_removePTags.Text = ui.Text("templateEditor", "removeParagraph");
            

            if (UmbracoSettings.UseAspNetMasterPages)
            {
                tagName.Value = "umbraco:Item";
            }

            // either get page fields or dictionary items
            string fieldSql = "";
            if (helper.Request("tagName") == "UMBRACOGETDICTIONARY")
            {
                fieldSql = "select '#'+[key] as alias from cmsDictionary order by alias";
                m_IsDictionaryMode = true;
                pp_insertField.Text = "Insert Dictionary Item";
            }
            else
            {
                fieldSql = "select distinct alias from cmsPropertyType order by alias";
                pp_insertField.Text = ui.Text("templateEditor", "chooseField");
            }

            fieldPicker.ChooseText = ui.Text("templateEditor", "chooseField");
            fieldPicker.StandardPropertiesLabel = ui.Text("templateEditor", "standardFields");
            fieldPicker.CustomPropertiesLabel = ui.Text("templateEditor", "customFields");
                        
            IRecordsReader dataTypes = SqlHelper.ExecuteReader(fieldSql);
			fieldPicker.DataTextField = "alias";
			fieldPicker.DataValueField = "alias";
			fieldPicker.DataSource = dataTypes;
			fieldPicker.DataBind();
            fieldPicker.Attributes.Add("onChange", "document.forms[0].field.value = document.forms[0]." + fieldPicker.ClientID + "[document.forms[0]." + fieldPicker.ClientID + ".selectedIndex].value;");
			dataTypes.Close();

            altFieldPicker.ChooseText = ui.Text("templateEditor", "chooseField");
            altFieldPicker.StandardPropertiesLabel = ui.Text("templateEditor", "standardFields");
            altFieldPicker.CustomPropertiesLabel = ui.Text("templateEditor", "customFields");
            
			IRecordsReader dataTypes2 = SqlHelper.ExecuteReader(fieldSql);
			altFieldPicker.DataTextField = "alias";
			altFieldPicker.DataValueField = "alias";
			altFieldPicker.DataSource = dataTypes2;
			altFieldPicker.DataBind();
            altFieldPicker.Attributes.Add("onChange", "document.forms[0].useIfEmpty.value = document.forms[0]." + altFieldPicker.ClientID + "[document.forms[0]." + altFieldPicker.ClientID + ".selectedIndex].value;");
			dataTypes2.Close();

			// Pre values
            if (!m_IsDictionaryMode)
            {
                foreach (string s in preValuesSource)
                {
                    fieldPicker.Items.Add(new ListItem(s, s.Replace("@", "")));
                    altFieldPicker.Items.Add(new ListItem(s, s.Replace("@", "")));
                }
            }

		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
		}
		#endregion
	}
}
