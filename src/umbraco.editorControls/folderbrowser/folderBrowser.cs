using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;

namespace umbraco.editorControls
{
    /// <summary>
    /// Summary description for folderBrowser.
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class folderBrowser : WebControl, interfaces.IDataEditor
    {
        public Control Editor { get { return this; } }

        public virtual bool TreatAsRichTextEditor { get { return false; } }
        public bool ShowLabel { get { return false; } }

        public void Save()
        {
        }

        private readonly string _usercontrolPath = IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dashboard/MediaDashboardFolderBrowser.ascx";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            base.Controls.Add(new UserControl().LoadControl(_usercontrolPath));
        }
        protected override void Render(HtmlTextWriter writer)
        {
            const string styles = "<style>.umbFolderBrowser { position: relative; min-height: 440px; } .upload-panel { top: 80px; margin: 0 0 0 -200px; } .umbFolderBrowser .filter, .umbFolderBrowser .thumb-sizer { top: 0; }</style>";
            writer.WriteLine(styles);
            base.Render(writer);
        }
    }
}
