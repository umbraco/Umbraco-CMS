using System;
using Umbraco.Core;

namespace umbraco.editorControls.label
{
	/// <summary>
	/// Summary description for DataTypeNoEdit.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class DataTypeNoEdit : cms.businesslogic.datatype.BaseDataType,interfaces.IDataType
	{

			private interfaces.IDataEditor _Editor;
		private interfaces.IData _baseData;
		private interfaces.IDataPrevalue _prevalueeditor;

		public override string DataTypeName	{get {return "No edit";}}
		public override Guid Id { get { return new Guid(Constants.PropertyEditors.NoEdit); } }

	
		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null)
					_Editor = new noEdit(Data);
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
