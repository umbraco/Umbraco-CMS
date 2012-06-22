using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.uicontrols.TreePicker
{
    public class SimpleContentPicker : BaseTreePicker
    {
        public override string TreePickerUrl
        {
            get
            {
                return TreeUrlGenerator.GetPickerUrl("content", "content");
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
