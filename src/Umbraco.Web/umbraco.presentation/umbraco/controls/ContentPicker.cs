using System;
using umbraco.uicontrols.TreePicker;
using Umbraco.Core;
using Umbraco.Core.Services;

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
                return ApplicationContext.Current.Services.TextService.Localize("general/choose") + " " + ApplicationContext.Current.Services.TextService.Localize("sections/" + TreeAlias.ToLower());
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
