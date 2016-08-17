using Umbraco.Core.Services;
using System;
using System.Collections;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;
using Umbraco.Web.UI;
using Umbraco.Web;
using Umbraco.Web._Legacy.UI;

namespace umbraco.cms.presentation.create.controls
{
    /// <summary>
    ///		Summary description for language.
    /// </summary>
    public partial class language : global::Umbraco.Web.UI.Controls.UmbracoUserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // get all existing languages

            pp1.Text = Services.TextService.Localize("choose") + " " + Services.TextService.Localize("language");
            sbmt.Text = Services.TextService.Localize("create");

            var sortedCultures = new SortedList();
            Cultures.Items.Clear();
            Cultures.Items.Add(new ListItem(Services.TextService.Localize("choose") + "...", ""));
            foreach (var cultureInfo in CultureInfo.GetCultures(CultureTypes.AllCultures))
                sortedCultures.Add(cultureInfo.DisplayName + "|||" + Guid.NewGuid(), cultureInfo.Name);

            var dictionaryEnumerator = sortedCultures.GetEnumerator();
            while (dictionaryEnumerator.MoveNext())
            {
                var language = dictionaryEnumerator.Key.ToString().Substring(0, dictionaryEnumerator.Key.ToString().IndexOf("|||", StringComparison.Ordinal));
                var listItem = new ListItem(string.Format("{0} [{1}]", language, dictionaryEnumerator.Value), dictionaryEnumerator.Value.ToString());
                Cultures.Items.Add(listItem);
            }
        }

        protected void sbmt_Click(object sender, EventArgs e)
        {
            LegacyDialogHandler.Create(
                new HttpContextWrapper(Context),
                Security.CurrentUser,
                Request.GetItemAsString("nodeType"),
                -1,
                Cultures.SelectedValue);

            ClientTools
                .ReloadActionNode(false, true)
                .CloseModalWindow();
        }

        /// <summary>
        /// pp1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp1;

        /// <summary>
        /// Cultures control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DropDownList Cultures;

        /// <summary>
        /// RequiredFieldValidator1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.RequiredFieldValidator RequiredFieldValidator1;

        /// <summary>
        /// sbmt control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button sbmt;

    }
}