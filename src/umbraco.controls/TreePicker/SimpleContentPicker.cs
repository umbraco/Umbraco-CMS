using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace umbraco.uicontrols.TreePicker
{
    public class SimpleContentPicker : BaseTreePicker
    {
        public override string TreePickerUrl
        {
            get
            {
                return Constants.Applications.Content;
            }
        }

        public override string ModalWindowTitle
        {
            get
            {
                return ApplicationContext.Current.Services.TextService.Localize("general/choose") + " " + ApplicationContext.Current.Services.TextService.Localize("sections/content");
            }
        }
    }
}
