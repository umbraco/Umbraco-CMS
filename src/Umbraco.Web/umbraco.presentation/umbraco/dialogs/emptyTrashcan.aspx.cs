using System;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Web;
using umbraco.BasePages;
using umbraco.cms.businesslogic;

namespace umbraco.presentation.dialogs
{
    public partial class emptyTrashcan : UmbracoEnsuredPage
    {
        private RecycleBin.RecycleBinType? _binType;
        protected RecycleBin.RecycleBinType BinType
        {
            get
            {
                if (_binType == null)
                {
                    _binType = Enum<RecycleBin.RecycleBinType>.Parse(Request.GetItemAsString("type"), true);
                }
                return _binType.Value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var recycleBinType = helper.Request("type");
            if (ValidateUserApp(recycleBinType) == false)
            {
                throw new InvalidOperationException("The user does not have access to the requested app");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/trashcan.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }

        /// <summary>
        /// pane_form control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_form;

        /// <summary>
        /// progbar control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.ProgressBar progbar;
    }
}