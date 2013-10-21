using System;
using Umbraco.Core;

namespace umbraco.editorControls.uploadfield
{
	/// <summary>
	/// Summary description for DataTypeUploadField.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class DataTypeUploadField : cms.businesslogic.datatype.BaseDataType, interfaces.IDataType
	{
		private interfaces.IDataEditor _editor;
		private interfaces.IData _baseData;
		private interfaces.IDataPrevalue _prevalueeditor;

		/// <summary>
		/// Always returns an uploadField control
		/// </summary>
		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_editor == null)
                    _editor = new uploadField(Data, ((uploadFieldPreValue)PrevalueEditor).Configuration);
				return _editor;
			}
		}

		/// <summary>
		/// Always returns FileHandlerData
		/// </summary>
		public override interfaces.IData Data 
		{
			get 
			{
				if (_baseData == null)
                    _baseData = new cms.businesslogic.datatype.FileHandlerData(this, ((uploadFieldPreValue)PrevalueEditor).Configuration);
				return _baseData;
			}
		}
		
		public override string DataTypeName 
		{
			get {return "Upload field";}
		}

		public override Guid Id 
		{
			get { return new Guid(Constants.PropertyEditors.UploadField); }
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
