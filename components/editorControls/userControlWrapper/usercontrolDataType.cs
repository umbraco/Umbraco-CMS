using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace umbraco.editorControls.userControlGrapper
{
    public class usercontrolDataType : umbraco.cms.businesslogic.datatype.BaseDataType, umbraco.interfaces.IDataType
    {
        private umbraco.interfaces.IDataEditor _Editor;
        private umbraco.interfaces.IData _baseData;
        private usercontrolPrevalueEditor _prevalueeditor;

        public override umbraco.interfaces.IDataEditor DataEditor
        {
            get
            {
                if (_Editor == null)
                    _Editor = new usercontrolDataEditor(Data, ((usercontrolPrevalueEditor) PrevalueEditor).Configuration );
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
            get { return new Guid("D15E1281-E456-4b24-AA86-1DDA3E4299D5"); }
        }

        public override string DataTypeName
        {
            get { return "umbraco usercontrol wrapper"; }
        }

        public override umbraco.interfaces.IDataPrevalue PrevalueEditor
        {
            get
            {
                if (_prevalueeditor == null)
                    _prevalueeditor = new usercontrolPrevalueEditor(this);
                return _prevalueeditor;
            }
        }
    }
}