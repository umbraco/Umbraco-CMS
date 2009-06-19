using System;

namespace umbraco.editorControls.mediapicker
{
	/// <summary>
	/// Summary description for MemberPickerDataType.
	/// </summary>
	public class MemberPickerDataType : cms.businesslogic.datatype.BaseDataType,interfaces.IDataType
	{
		private interfaces.IDataEditor _Editor;
		private interfaces.IData _baseData;
		private interfaces.IDataPrevalue _prevalueeditor;

		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null)
					_Editor = new mediaChooser(Data);
				return _Editor;
			}
		}

		public override interfaces.IData Data 
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
			get
			{
				return new Guid ("EAD69342-F06D-4253-83AC-28000225583B");
			}
		}
		public override string DataTypeName
		{
			get
			{
				return "Media Picker";
			}
		}
		public override interfaces.IDataPrevalue PrevalueEditor 
		{
			get 
			{
				if (_prevalueeditor == null)
					_prevalueeditor = new DefaultPrevalueEditor(this,false);
				return _prevalueeditor;
			}
		}
	}
}
