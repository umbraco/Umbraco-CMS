using System;

namespace umbraco.editorControls.simpleEditor
{
	/// <summary>
	/// Summary description for simpleEditorDataType.
	/// </summary>
	public class simpleEditorDataType : cms.businesslogic.datatype.BaseDataType,interfaces.IDataType
	{
		private interfaces.IDataEditor _Editor;
		private interfaces.IData _baseData;
		private interfaces.IDataPrevalue _prevalueeditor;

		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null)
					_Editor = new SimpleEditor(Data);
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
			get {return new Guid("60B7DABF-99CD-41eb-B8E9-4D2E669BBDE9");}
		}

		public override string DataTypeName 
		{
			get {return "UltraSimpleEditor";}
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