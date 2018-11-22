using System.Linq;
using System.Web.UI.WebControls;
using Umbraco.Core.Configuration;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Composing;

namespace umbraco.dialogs
{
    /// <summary>
    /// Summary description for umbracoField.
    /// </summary>
    public partial class umbracoField : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {
        string[] preValuesSource = { "@createDate", "@creatorName", "@level", "@nodeType", "@nodeTypeAlias", "@pageID", "@pageName", "@parentID", "@path", "@template", "@updateDate", "@writerID", "@writerName" };
        bool m_IsDictionaryMode = false;

        public umbracoField()
        {
            CurrentApp = Constants.Applications.Settings.ToString();
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {

            //set labels on properties...
            pp_insertAltField.Text = Services.TextService.Localize("templateEditor/alternativeField");
            pp_insertAltText.Text = Services.TextService.Localize("templateEditor/alternativeText");
            pp_insertBefore.Text = Services.TextService.Localize("templateEditor/preContent");
            pp_insertAfter.Text = Services.TextService.Localize("templateEditor/postContent");

            pp_FormatAsDate.Text = Services.TextService.Localize("templateEditor/formatAsDate");
            pp_casing.Text = Services.TextService.Localize("templateEditor/casing");
            pp_encode.Text = Services.TextService.Localize("templateEditor/encoding");

            tagName.Value = "umbraco:Item";

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                // either get page fields or dictionary items
                string fieldSql = "";
                if (Request.GetItemAsString("tagName") == "UMBRACOGETDICTIONARY")
                {
                    fieldSql = "select '#'+[key] as alias from cmsDictionary order by alias";
                    m_IsDictionaryMode = true;
                    pp_insertField.Text = "Insert Dictionary Item";
                }
                else
                {
                    //exclude built-in memberhip properties from showing up here
                    var exclude = Constants.Conventions.Member.GetStandardPropertyTypeStubs()
                        .Select(x => Current.SqlContext.SqlSyntax.GetQuotedValue(x.Key)).ToArray();

                    fieldSql = string.Format(
                        "select distinct alias from cmsPropertyType where alias not in ({0}) order by alias",
                        string.Join(",", exclude));
                    pp_insertField.Text = Services.TextService.Localize("templateEditor/chooseField");
                }

                fieldPicker.ChooseText = Services.TextService.Localize("templateEditor/chooseField");
                fieldPicker.StandardPropertiesLabel = Services.TextService.Localize("templateEditor/standardFields");
                fieldPicker.CustomPropertiesLabel = Services.TextService.Localize("templateEditor/customFields");

                var dataTypes = scope.Database.Fetch<dynamic>(fieldSql);
                fieldPicker.DataTextField = "alias";
                fieldPicker.DataValueField = "alias";
                fieldPicker.DataSource = dataTypes;
                fieldPicker.DataBind();
                fieldPicker.Attributes.Add("onChange", "document.forms[0].field.value = document.forms[0]." + fieldPicker.ClientID + "[document.forms[0]." + fieldPicker.ClientID + ".selectedIndex].value;");

                altFieldPicker.ChooseText = Services.TextService.Localize("templateEditor/chooseField");
                altFieldPicker.StandardPropertiesLabel = Services.TextService.Localize("templateEditor/standardFields");
                altFieldPicker.CustomPropertiesLabel = Services.TextService.Localize("templateEditor/customFields");

                var dataTypes2 = scope.Database.Fetch<dynamic>(fieldSql);
                altFieldPicker.DataTextField = "alias";
                altFieldPicker.DataValueField = "alias";
                altFieldPicker.DataSource = dataTypes2;
                altFieldPicker.DataBind();
                altFieldPicker.Attributes.Add("onChange", "document.forms[0].useIfEmpty.value = document.forms[0]." + altFieldPicker.ClientID + "[document.forms[0]." + altFieldPicker.ClientID + ".selectedIndex].value;");

                scope.Complete();
            }

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
        protected global::Umbraco.Web._Legacy.Controls.Pane pane_form;

        /// <summary>
        /// pp_insertField control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::Umbraco.Web._Legacy.Controls.PropertyPanel pp_insertField;

        /// <summary>
        /// fieldPicker control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::Umbraco.Web._Legacy.Controls.FieldDropDownList fieldPicker;

        /// <summary>
        /// pp_insertAltField control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::Umbraco.Web._Legacy.Controls.PropertyPanel pp_insertAltField;

        /// <summary>
        /// altFieldPicker control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::Umbraco.Web._Legacy.Controls.FieldDropDownList altFieldPicker;

        /// <summary>
        /// pp_insertAltText control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::Umbraco.Web._Legacy.Controls.PropertyPanel pp_insertAltText;

        /// <summary>
        /// pp_recursive control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::Umbraco.Web._Legacy.Controls.PropertyPanel pp_recursive;

        /// <summary>
        /// pp_insertBefore control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::Umbraco.Web._Legacy.Controls.PropertyPanel pp_insertBefore;

        /// <summary>
        /// pp_insertAfter control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::Umbraco.Web._Legacy.Controls.PropertyPanel pp_insertAfter;

        /// <summary>
        /// pp_FormatAsDate control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::Umbraco.Web._Legacy.Controls.PropertyPanel pp_FormatAsDate;

        /// <summary>
        /// pp_casing control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::Umbraco.Web._Legacy.Controls.PropertyPanel pp_casing;

        /// <summary>
        /// pp_encode control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::Umbraco.Web._Legacy.Controls.PropertyPanel pp_encode;

        /// <summary>
        /// pp_convertLineBreaks control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::Umbraco.Web._Legacy.Controls.PropertyPanel pp_convertLineBreaks;

        /// <summary>
        /// pp_removePTags control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::Umbraco.Web._Legacy.Controls.PropertyPanel pp_removePTags;
    }
}
