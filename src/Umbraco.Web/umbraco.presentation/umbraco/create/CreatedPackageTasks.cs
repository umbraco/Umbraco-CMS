using System;
using System.Data;
using System.Web.Security;
using Umbraco.Core.Logging;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class CreatedPackageTasks : LegacyDialogTask
    {
        
        public override bool PerformSave()
        {
            LogHelper.Info<CreatedPackageTasks>("Xml save started");
            int id = cms.businesslogic.packager.CreatedPackage.MakeNew(Alias).Data.Id;
            _returnUrl = string.Format("developer/packages/editPackage.aspx?id={0}", id);
            return true;
        }

        public override bool PerformDelete()
        {
            // we need to grab the id from the alias as the new tree needs to prefix the NodeID with "package_"
            if (ParentID == 0)
            {
                ParentID = int.Parse(Alias.Substring(loadPackages.PACKAGE_TREE_PREFIX.Length));
            }
            cms.businesslogic.packager.CreatedPackage.GetById(ParentID).Delete();
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
