using System;
using System.Data;
using System.Web.Security;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class DataTypeTasks : LegacyDialogTask
    {

        public override bool PerformSave()
        {
            var id = cms.businesslogic.datatype.DataTypeDefinition.MakeNew(User, Alias).Id;
            _returnUrl = string.Format("developer/datatypes/editDataType.aspx?id={0}", id);
            return true;
        }

        public override bool PerformDelete()
        {
            cms.businesslogic.datatype.DataTypeDefinition.GetDataTypeDefinition(ParentID).delete();
            return true;
        }
        
        private string _returnUrl = "";

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return DefaultApps.developer.ToString(); }
        }
    }
}
