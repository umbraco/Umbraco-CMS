using System;

namespace umbraco.editorControls.folderbrowser
{
	/// <summary>
	/// Summary description for DataTypeFolderbrowser.
	/// </summary>
	/// <summary>
	/// Summary description for DataTypeUploadField.
	/// </summary>
	public class DataTypeFolderBrowser: cms.businesslogic.datatype.BaseDataType,interfaces.IDataType
	{
		private interfaces.IDataEditor _Editor;
		private cms.businesslogic.datatype.DefaultData _baseData;
		private interfaces.IDataPrevalue _prevalueeditor;

		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null)
                    _Editor = new folderBrowser((umbraco.cms.businesslogic.datatype.DefaultData)Data);
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
		
		public override Guid Id
		{
			get
			{
				return new Guid ("CCCD4AE9-F399-4ED2-8038-2E88D19E810C");
			}
		}

		public override string DataTypeName
		{
			get
			{
				return "Folder browser";
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
