using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace umbraco.editorControls.ultimatepicker
{
    public class ultimatePickerDataType : umbraco.cms.businesslogic.datatype.BaseDataType, umbraco.interfaces.IDataType
    {
        private umbraco.interfaces.IDataEditor _Editor;
        private umbraco.interfaces.IData _baseData;
        private ultimatePickerPrevalueEditor _prevalueeditor;

        public override umbraco.interfaces.IDataEditor DataEditor
        {
            get
            {
                if (_Editor == null)
                    _Editor = new ultimatePickerDataEditor(Data, ((ultimatePickerPrevalueEditor)PrevalueEditor).Configuration);
                return _Editor;
            }
        }

        public override umbraco.interfaces.IData Data
        {
            get
            {
                if (_baseData == null)
                    _baseData = new umbraco.cms.businesslogic.datatype.DefaultData(this);
                return _baseData;
            }
        }
        public override Guid Id
        {
            get { return new Guid("cdbf0b5d-5cb2-445f-bc12-fcaaec07cf2c"); }
        }

        public override string DataTypeName
        {
            get { return "Ultimate Picker"; }
        }

        public override umbraco.interfaces.IDataPrevalue PrevalueEditor
        {
            get
            {
                if (_prevalueeditor == null)
                    _prevalueeditor = new ultimatePickerPrevalueEditor(this);
                return _prevalueeditor;
            }
        }
    }
}
