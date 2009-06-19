using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.editorControls.tinyMCE3
{
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
            get { return new Guid("{5E9B75AE-FACE-41c8-B47E-5F4B0FD82F83}"); }
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
