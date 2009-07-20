using System;

namespace umbraco.editorControls.wysiwyg
{
	/// <summary>
	/// Summary description for WysiwygDataType.
	/// </summary>
	public class WysiwygDataType : cms.businesslogic.datatype.BaseDataType,interfaces.IDataType
	{
		private editor _Editor;
		private cms.businesslogic.datatype.DefaultData _baseData;
		private interfaces.IDataPrevalue _prevalueeditor;

		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null) 
				{
                    _Editor = new editor((cms.businesslogic.datatype.DefaultData)Data);
				
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
			get {return new Guid("a3776494-0574-4d93-b7de-efdfdec6f2d1");}
		}

		public override string DataTypeName 
		{
			get {return "Editor";}
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
