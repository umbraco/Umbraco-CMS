using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;

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
                return ui.GetText("general", "choose") + " " + ui.GetText("sections", "content");
            }
        }
    }
}
