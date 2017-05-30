using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Composing;
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
                return Current.Services.TextService.Localize("general/choose") + " " + Current.Services.TextService.Localize("sections/content");
            }
        }
    }
}
