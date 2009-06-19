using System;

namespace umbraco.editorControls.radiobuttonlist
{
	/// <summary>
	/// Summary description for ColorPickerDataType.
	/// </summary>
	public class RadioButtonListDataType : cms.businesslogic.datatype.BaseDataType, interfaces.IDataType
	{
		private interfaces.IDataEditor _Editor;
		private interfaces.IData _baseData;
		private KeyValuePrevalueEditor _prevalueeditor;

		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null) 
				{
					_Editor = new radiobox(Data,((KeyValuePrevalueEditor)PrevalueEditor).Prevalues);
				}
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
		public override string DataTypeName 
		{
			get {return "Radiobutton list";}
		}
		public override Guid Id 
		{
			get {return new Guid("A52C7C1C-C330-476E-8605-D63D3B84B6A6");}
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
