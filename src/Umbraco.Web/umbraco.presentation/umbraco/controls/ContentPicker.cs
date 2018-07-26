using System;
using Umbraco.Web._Legacy.Controls;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Composing;

namespace umbraco.controls
{

    public class ContentPicker : BaseTreePicker
    {

        public ContentPicker()
        {
            AppAlias = Constants.Applications.Content;
            TreeAlias = "content";
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
                return Current.Services.TextService.Localize("general/choose") + " " + Current.Services.TextService.Localize("sections/" + TreeAlias.ToLower());
            }
        }

        protected override string GetItemTitle()
        {
            string tempTitle = "";
            try
            {
                if (Value != "" && Value != "-1")
                {
                    //tempTitle = new cms.businesslogic.CMSNode(int.Parse(Value)).Text;
                    tempTitle = Current.Services.EntityService.Get(int.Parse(Value)).Name;
                }
                else
                {
                    tempTitle = (!string.IsNullOrEmpty(TreeAlias) ? Current.Services.TextService.Localize(TreeAlias) : Current.Services.TextService.Localize(AppAlias));

                }
            }
            catch { }
            return tempTitle;
        }
    }
}
