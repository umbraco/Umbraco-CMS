using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.editorControls.wysiwyg
{
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
