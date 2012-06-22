using System;

namespace umbraco.editorControls.numberfield
{
	/// <summary>
	/// Summary description for IDataTypenteger.
	/// </summary>
	public class IDataTypenteger : cms.businesslogic.datatype.BaseDataType,interfaces.IDataType
	{
		private interfaces.IDataEditor _Editor;
		private interfaces.IData _baseData;
		private interfaces.IDataPrevalue _prevalueeditor;

		public override string DataTypeName 
		{
			get {return "Integer";}
		}

		public override Guid Id 
		{
			get {return new Guid("1413afcb-d19a-4173-8e9a-68288d2a73b8");}
		}

		public override interfaces.IDataPrevalue PrevalueEditor 
		{
			get 
			{
				if (_prevalueeditor == null)
					_prevalueeditor = new DefaultPrevalueEditor(this,true);
				return _prevalueeditor;
			}
		}

		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null)
					_Editor = new numberField(Data);
				return _Editor;
			}
		}

		public override interfaces.IData Data 
		{
			get 
			{
				if (_baseData == null)
					_baseData = new DataInteger(this);
				return _baseData;
			}
		}

	}
}
