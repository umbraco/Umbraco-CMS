using System;

namespace umbraco.editorControls.label
{
	/// <summary>
	/// Summary description for DataTypeNoEdit.
	/// </summary>
	public class DataTypeNoEdit : cms.businesslogic.datatype.BaseDataType,interfaces.IDataType
	{

			private interfaces.IDataEditor _Editor;
		private interfaces.IData _baseData;
		private interfaces.IDataPrevalue _prevalueeditor;

		public override string DataTypeName	{get {return "No edit";}}
		public override Guid Id {get {return new Guid("6c738306-4c17-4d88-b9bd-6546f3771597");}}

	
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
