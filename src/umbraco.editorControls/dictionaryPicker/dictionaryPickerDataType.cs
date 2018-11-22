using System;
using Umbraco.Core;

namespace umbraco.editorControls.dictionaryPicker
{
	/// <summary>
	/// Summary description for dictionaryPickerDataType.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class dictionaryPickerDataType : cms.businesslogic.datatype.BaseDataType, interfaces.IDataType
	{

		private interfaces.IDataEditor _Editor;
		private interfaces.IData _baseData;
		private interfaces.IDataPrevalue _prevalueeditor;
		
		public dictionaryPickerDataType()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		#region IDataType Members

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
			get
			{
				// TODO:  Add dictionaryPickerDataType.Id getter implementation
				return new Guid(Constants.PropertyEditors.DictionaryPicker);
			}
		}

		public override umbraco.interfaces.IDataEditor DataEditor
		{
			get
			{
				if (_Editor == null)
					_Editor = new dictionaryPicker(Data, ((KeyValuePrevalueEditor)PrevalueEditor).Prevalues);
				return _Editor;
			}
		}


		public override string DataTypeName
		{
			get
			{
				// TODO:  Add dictionaryPickerDataType.DataTypeName getter implementation
				return "Dictionary Picker";
			}
		}

		public override umbraco.interfaces.IDataPrevalue PrevalueEditor
		{
			get
			{
				if (_prevalueeditor == null)
					_prevalueeditor = new KeyValuePrevalueEditor(this);
				return _prevalueeditor;
			}
		}

		#endregion
	}
}
