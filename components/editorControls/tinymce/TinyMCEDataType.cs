using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.editorControls.tinymce
{
	public class TinyMCEDataType : umbraco.cms.businesslogic.datatype.BaseDataType, umbraco.interfaces.IDataType
	{
		private umbraco.interfaces.IDataEditor _Editor;
		private umbraco.interfaces.IData _baseData;
		private umbraco.interfaces.IDataPrevalue _prevalueeditor;

		public override umbraco.interfaces.IDataEditor DataEditor
		{
			get
			{
				if (_Editor == null)
                    _Editor = new TinyMCE(Data, ((tinyMCEPreValueConfigurator)PrevalueEditor).Configuration);
				return _Editor;
			}
		}

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
			get { return new Guid("{83722133-F80C-4273-BDB6-1BEFAA04A612}"); }
		}

		public override string DataTypeName
		{
			get { return "TinyMCE wysiwyg (deprecated, upgrade to tinymce v3!)"; }
		}

		public override umbraco.interfaces.IDataPrevalue PrevalueEditor
		{
			get
			{
                if (_prevalueeditor == null)
                    _prevalueeditor = new tinymce.tinyMCEPreValueConfigurator(this);
				return _prevalueeditor;
			}
		}
	}
}
