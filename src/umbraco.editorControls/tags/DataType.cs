using System;
using umbraco.interfaces;
using Umbraco.Core;
using datatype = umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.tags
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
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
            get { return new Guid(Constants.PropertyEditors.Tags); }
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
