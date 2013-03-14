using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;

namespace umbraco.editorControls.SettingControls.Pickers
{

    public class DocumentType: BasePicker {
        public DocumentType() {
            ObjectGuid = new Guid(Constants.ObjectTypes.DocumentType);
        }
    }
}