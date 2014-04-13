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
using Umbraco.Core;
using Umbraco.Core.Persistence.SqlSyntax;
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
			pp_insertBefore.Text = ui.Text("templateEditor", "preContent");
			pp_insertAfter.Text = ui.Text("templateEditor", "postContent");

			pp_FormatAsDate.Text = ui.Text("templateEditor", "formatAsDate");
			pp_casing.Text = ui.Text("templateEditor", "casing");
			pp_encode.Text = ui.Text("templateEditor", "encoding");
			


			if (UmbracoConfig.For.UmbracoSettings().Templates.UseAspNetMasterPages)
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
                //exclude built-in memberhip properties from showing up here
			    var exclude = Constants.Conventions.Member.GetStandardPropertyTypeStubs()
                    .Select(x => SqlSyntaxContext.SqlSyntaxProvider.GetQuotedValue(x.Key)).ToArray();

				fieldSql = string.Format(
                    "select distinct alias from cmsPropertyType where alias not in ({0}) order by alias",
                    string.Join(",", exclude));
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


		/// <summary>
		/// JsInclude1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

		/// <summary>
		/// JsInclude2 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude2;

		/// <summary>
		/// tagName control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.HtmlControls.HtmlInputHidden tagName;

		/// <summary>
		/// pane_form control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane pane_form;

		/// <summary>
		/// pp_insertField control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_insertField;

		/// <summary>
		/// fieldPicker control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.FieldDropDownList fieldPicker;

		/// <summary>
		/// pp_insertAltField control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_insertAltField;

		/// <summary>
		/// altFieldPicker control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.FieldDropDownList altFieldPicker;

		/// <summary>
		/// pp_insertAltText control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_insertAltText;

		/// <summary>
		/// pp_recursive control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_recursive;

		/// <summary>
		/// pp_insertBefore control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_insertBefore;

		/// <summary>
		/// pp_insertAfter control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_insertAfter;

		/// <summary>
		/// pp_FormatAsDate control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_FormatAsDate;

		/// <summary>
		/// pp_casing control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_casing;

		/// <summary>
		/// pp_encode control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_encode;

		/// <summary>
		/// pp_convertLineBreaks control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_convertLineBreaks;

		/// <summary>
		/// pp_removePTags control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_removePTags;
	}
}
