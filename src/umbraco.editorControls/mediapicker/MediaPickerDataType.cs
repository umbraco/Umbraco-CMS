using System;
using Umbraco.Core;

namespace umbraco.editorControls.mediapicker
{
    //TODO: Properly rename this for a major release
    public class MediaPickerDataType : MemberPickerDataType
    { }

    [Obsolete("Renamed to MediaPickerDataType because.. that is what it was all along")]
    public class MemberPickerDataType : cms.businesslogic.datatype.BaseDataType, interfaces.IDataType
    {
        private interfaces.IDataEditor _editor;
        private interfaces.IData _baseData;
        private interfaces.IDataPrevalue _prevalueeditor;

        public override interfaces.IDataEditor DataEditor
        {
            get { return _editor ?? (_editor = new mediaChooser(Data, ((MediaPickerPrevalueEditor)PrevalueEditor).ShowPreview, ((MediaPickerPrevalueEditor)PrevalueEditor).ShowAdvancedDialog)); }
        }

        public override interfaces.IData Data
        {
            get { return _baseData ?? (_baseData = new cms.businesslogic.datatype.DefaultData(this)); }
        }


		public override Guid Id
		{
			get
			{
				return new Guid(Constants.PropertyEditors.MediaPicker);
			}
		}


        public override string DataTypeName
        {
            get { return "Media Picker"; }
        }

        public override interfaces.IDataPrevalue PrevalueEditor
        {
            get { return _prevalueeditor ?? (_prevalueeditor = new MediaPickerPrevalueEditor(this)); }
        }
    }
}
