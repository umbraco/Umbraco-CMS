using System;

namespace umbraco.editorControls.textfieldmultiple
{
	public class textfieldMultipleDataType : cms.businesslogic.datatype.BaseDataType,interfaces.IDataType
	{
		private textfield.TextFieldEditor _textFieldEditor;
		private interfaces.IData _baseData;
		private interfaces.IDataPrevalue _prevalueeditor;

		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_textFieldEditor == null) 
				{
					_textFieldEditor = new textfield.TextFieldEditor(Data);
					_textFieldEditor.TextMode = System.Web.UI.WebControls.TextBoxMode.MultiLine;
					_textFieldEditor.Rows = 10;
					_textFieldEditor.Columns = 40;
					_textFieldEditor.CssClass = "umbEditorTextFieldMultiple";
				}
				return _textFieldEditor;
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
			get {return new Guid("67db8357-ef57-493e-91ac-936d305e0f2a");}
		}

		public override string DataTypeName 
		{
			get {return "Textbox multiple";}
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
