using System;

namespace umbraco.editorControls.checkboxlist
{
	public class checkboxListDataType : cms.businesslogic.datatype.BaseDataType, interfaces.IDataType
	{
		private interfaces.IDataEditor _Editor;
		private interfaces.IData _baseData;
		private KeyValuePrevalueEditor _prevalueeditor;

		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null) 
					_Editor = new checkboxlistEditor(Data,((KeyValuePrevalueEditor)PrevalueEditor).Prevalues);
				return _Editor;
			}
		}

		public override interfaces.IData Data 
		{
			get 
			{
				if (_baseData == null)
					_baseData = new DefaultDataKeyValue(this);
				return _baseData;
			}
		}

		public override string DataTypeName 
		{get {return "Checkbox list";}}

		public override Guid Id 
		{
			get {return new Guid("b4471851-82b6-4c75-afa4-39fa9c6a75e9");}
		}

		public override interfaces.IDataPrevalue PrevalueEditor 
		{
			get 
			{
				if (_prevalueeditor == null)
					_prevalueeditor = new KeyValuePrevalueEditor(this);
				return _prevalueeditor;
			}
		}
	}
}