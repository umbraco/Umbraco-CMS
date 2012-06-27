using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.editorControls.macrocontainer
{
    public class DataType : cms.businesslogic.datatype.BaseDataType, interfaces.IDataType
    {

        private interfaces.IDataEditor _Editor;
        private interfaces.IData _baseData;
        private interfaces.IDataPrevalue _prevalueeditor;

        public override interfaces.IDataEditor DataEditor
        {
            get
            {
                if (_Editor == null)
                    _Editor = new Editor(Data, ((PrevalueEditor)PrevalueEditor).AllowedMacros,
                        (int)((PrevalueEditor)PrevalueEditor).MaxNumber,
                        (int)((PrevalueEditor)PrevalueEditor).PreferedHeight,
                        (int)((PrevalueEditor)PrevalueEditor).PreferedWidth);
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
                return new Guid("474FCFF8-9D2D-11DE-ABC6-AD7A56D89593");
            }
        }
        public override string DataTypeName
        {
            get
            {
                return "Macro Container";
            }
        }
        public override interfaces.IDataPrevalue PrevalueEditor
        {
            get
            {
                if (_prevalueeditor == null)
                    _prevalueeditor = new PrevalueEditor(this);
                return _prevalueeditor;
            }
        }

    }
}
