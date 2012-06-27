using System;
using umbraco.interfaces;
using datatype = umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.tags
{
    public class DataType : datatype.BaseDataType, IDataType
    {
        #region IDataType Members

        private IDataEditor _Editor;
        private IData _baseData;
        private IDataPrevalue _prevalueeditor;

        public override IData Data
        {
            get {
                if (_baseData == null)
                    _baseData = new datatype.DefaultData(this);
                return _baseData;
            }
        }

        public override IDataEditor DataEditor
        {
            get
            {
                if (_Editor == null)
                {
                    _Editor = new DataEditor(Data, ((PrevalueEditor)PrevalueEditor).Prevalues);
                }
                return _Editor;
           }
        }

        public override string DataTypeName
        {
            get { return "Tags"; }
        }

        public override Guid Id
        {
            get { return new Guid("4023e540-92f5-11dd-ad8b-0800200c9a66"); }
        }

        public override IDataPrevalue PrevalueEditor
        {
            get
            {
                if (_prevalueeditor == null)
                    _prevalueeditor = new PrevalueEditor(this);
                return _prevalueeditor;
            }
        }

        #endregion
    }
}
