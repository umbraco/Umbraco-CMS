using System;
using Umbraco.Core;

namespace umbraco.editorControls.folderbrowser
{
    /// <summary>
    /// Summary description for DataTypeFolderbrowser.
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class DataTypeFolderBrowser : cms.businesslogic.datatype.BaseDataType, interfaces.IDataType
    {
        private interfaces.IDataEditor _editor;
        private cms.businesslogic.datatype.DefaultData _baseData;
        private interfaces.IDataPrevalue _prevalueeditor;

        public override interfaces.IDataEditor DataEditor
        {
            get { return _editor ?? (_editor = new folderBrowser()); }
        }

        public override interfaces.IData Data
        {
            get { return _baseData ?? (_baseData = new cms.businesslogic.datatype.DefaultData(this)); }
        }

        public override Guid Id { get { return new Guid(Constants.PropertyEditors.FolderBrowser); } }

        public override string DataTypeName { get { return "Folder browser"; } }

        public override interfaces.IDataPrevalue PrevalueEditor
        {
            get { return _prevalueeditor ?? (_prevalueeditor = new DefaultPrevalueEditor(this, false)); }
        }
    }
}
