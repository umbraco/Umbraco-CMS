using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;

namespace umbraco.editorControls.SettingControls.Pickers
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class DocumentType: BasePicker {
        public DocumentType() {
            ObjectGuid = new Guid(Constants.ObjectTypes.DocumentType);
        }
    }
}