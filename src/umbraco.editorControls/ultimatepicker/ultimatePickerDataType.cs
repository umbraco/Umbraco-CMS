using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Umbraco.Core;

namespace umbraco.editorControls.ultimatepicker
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
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
            get { return new Guid(Constants.PropertyEditors.UltimatePicker); }
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
