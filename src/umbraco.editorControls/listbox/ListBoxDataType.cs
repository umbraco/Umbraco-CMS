using System;
using Umbraco.Core;

namespace umbraco.editorControls.listbox
{
	/// <summary>
	/// Summary description for ColorPickerDataType.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class ListBoxDataType : cms.businesslogic.datatype.BaseDataType, interfaces.IDataType
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
					_Editor = new dropdownMultiple(Data,((KeyValuePrevalueEditor)PrevalueEditor).PrevaluesAsKeyValuePairList);
				}
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
		{
			get {return "Dropdown list multiple";}
		}

		public override Guid Id 
		{
			get { return new Guid(Constants.PropertyEditors.DropDownListMultiple); }
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
