using System;
using Umbraco.Core;

namespace umbraco.editorControls.memberpicker
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class MemberPickerDataType : cms.businesslogic.datatype.BaseDataType, interfaces.IDataType
    {
        private interfaces.IDataEditor _editor;
        private interfaces.IData _baseData;
        private interfaces.IDataPrevalue _prevalueeditor;

        public override interfaces.IDataEditor DataEditor
        {
            get { return _editor ?? (_editor = new memberPicker(Data)); }
        }

        public override interfaces.IData Data
        {
            get { return _baseData ?? (_baseData = new cms.businesslogic.datatype.DefaultData(this)); }
        }
        public override string DataTypeName
        {
            get { return "Member Picker"; }
        }

        public override Guid Id
        {
			get { return new Guid(Constants.PropertyEditors.MemberPicker); }
        }

        public override interfaces.IDataPrevalue PrevalueEditor
        {
            get { return _prevalueeditor ?? (_prevalueeditor = new DefaultPrevalueEditor(this, false)); }
        }
    }
}
