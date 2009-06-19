using System;

namespace umbraco.editorControls.colorpicker
{
	/// <summary>
	/// Summary description for ColorPickerDataType.
	/// </summary>
	public class ColorPickerDataType : cms.businesslogic.datatype.BaseDataType, interfaces.IDataType
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
					_Editor = new colorPicker(Data,((KeyValuePrevalueEditor)PrevalueEditor).Prevalues);
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
			get {return "Color Picker";}
		}
		public override Guid Id 
		{
			get {return new Guid("F8D60F68-EC59-4974-B43B-C46EB5677985");}
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
