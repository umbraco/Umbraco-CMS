using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core;

namespace umbraco.editorControls.tinyMCE3
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class tinyMCE3dataType : umbraco.cms.businesslogic.datatype.BaseDataType, umbraco.interfaces.IDataType
    {
        private umbraco.interfaces.IDataEditor _Editor;
        private umbraco.interfaces.IData _baseData;
        private umbraco.interfaces.IDataPrevalue _prevalueeditor;

        public override umbraco.interfaces.IDataEditor DataEditor
        {
            get
            {
                if (_Editor == null)
                    _Editor = new TinyMCE(Data, ((umbraco.editorControls.tinymce.tinyMCEPreValueConfigurator)PrevalueEditor).Configuration);
                return _Editor;
            }
        }

        public override umbraco.interfaces.IData Data
        {
            get
            {
                if (_baseData == null)
                    _baseData = new cms.businesslogic.datatype.DefaultData(this);
                return _baseData;
            }
        }
        public override Guid Id
        {
            get { return new Guid(Constants.PropertyEditors.TinyMCEv3); }
        }

        public override string DataTypeName
        {
            get { return "TinyMCE v3 wysiwyg"; }
        }

        public override umbraco.interfaces.IDataPrevalue PrevalueEditor
        {
            get
            {
                if (_prevalueeditor == null)
                    _prevalueeditor = new tinymce.tinyMCEPreValueConfigurator(this);
                return _prevalueeditor;
            }
        }
    }
}
