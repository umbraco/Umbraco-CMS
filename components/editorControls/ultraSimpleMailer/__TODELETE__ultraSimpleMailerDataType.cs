using System;

namespace umbraco.editorControls.ultraSimpleMailer
{
	/// <summary>
	/// Summary description for ultraSimpleMailerDataType.
	/// </summary>
	public class ultraSimpleMailerDataType : cms.businesslogic.datatype.BaseDataType,interfaces.IDataType
	{
		private ultraSimpleMailerEditor _Editor;
		private cms.businesslogic.datatype.DefaultData _baseData;
		private mailerConfiguratorPreValueEditor _prevalueeditor;

		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null) 
				{
                    _Editor = new ultraSimpleMailerEditor((umbraco.cms.businesslogic.datatype.DefaultData)Data, ((mailerConfiguratorPreValueEditor)PrevalueEditor).Configuration);
				
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
		public override Guid Id 
		{
			get {return new Guid("AABE748C-EFB6-4225-B7B2-DABC6FE36945");}
		}

		public override string DataTypeName 
		{
			get {return "UltraSimpleMailer(tm)";}
		}

		public override interfaces.IDataPrevalue PrevalueEditor 
		{
			get 
			{
				if (_prevalueeditor == null)
					_prevalueeditor = new mailerConfiguratorPreValueEditor(this);
				return _prevalueeditor;
			}
		}
	}
}
