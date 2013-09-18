using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using umbraco.cms.presentation.Trees;
using umbraco.presentation;
using umbraco.uicontrols.TreePicker;
using Umbraco.Core;

namespace umbraco.controls
{

    public class ContentPicker : BaseTreePicker
	{

        public ContentPicker()
        {
            AppAlias = Constants.Applications.Content;
            TreeAlias = "content";
        }

		[Obsolete("Use Value property instead, this simply wraps it.")]
		public string Text
		{
			get
			{
                return this.Value;
			}
            set
            {
                this.Value = value;
            }
		}

        public string AppAlias { get; set; }
        public string TreeAlias { get; set; }

        public override string TreePickerUrl
        {
            get
            {
                return AppAlias;
            }
        }

        public override string ModalWindowTitle
        {
            get
            {
                return ui.GetText("general", "choose") + " " + ui.GetText("sections", TreeAlias.ToLower());
            }
        }

        protected override string GetItemTitle()
        {
            string tempTitle = "";
            try
            {
                if (Value != "" && Value != "-1")
                {
                    tempTitle = new cms.businesslogic.CMSNode(int.Parse(Value)).Text;
                }
                else
                {
                    tempTitle = (!string.IsNullOrEmpty(TreeAlias) ? ui.Text(TreeAlias) : ui.Text(AppAlias));

                }
            }
            catch { }
            return tempTitle;
        }
	}
}
