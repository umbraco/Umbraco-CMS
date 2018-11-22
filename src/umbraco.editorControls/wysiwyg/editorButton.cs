using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.editorControls.wysiwyg
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class editorButton
    {
        public string imageUrl;
        public string onClickCommand;
        public string alttag;
        public string id;

        public editorButton(string Id, string AltTag, string ImageUrl, string OnClickCommand)
        {
            id = Id;
            alttag = AltTag;
            imageUrl = ImageUrl;
            onClickCommand = OnClickCommand;
        }
    }
}
