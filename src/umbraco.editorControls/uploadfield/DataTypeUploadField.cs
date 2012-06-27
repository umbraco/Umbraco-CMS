using System;

namespace umbraco.editorControls.uploadfield
{
	/// <summary>
	/// Summary description for DataTypeUploadField.
	/// </summary>
	public class DataTypeUploadField : cms.businesslogic.datatype.BaseDataType,interfaces.IDataType
	{
		private interfaces.IDataEditor _Editor;
		private interfaces.IData _baseData;
		private interfaces.IDataPrevalue _prevalueeditor;

		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null)
                    _Editor = new uploadField(Data, ((uploadFieldPreValue)PrevalueEditor).Configuration);
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
			get {return "Upload field";}
		}

		public override Guid Id 
		{
			get {return new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c");}
		}

		public override interfaces.IDataPrevalue PrevalueEditor 
		{
			get 
			{
				if (_prevalueeditor == null)
					_prevalueeditor = new uploadFieldPreValue(this);
				return _prevalueeditor;
			}
		}
	}
}
