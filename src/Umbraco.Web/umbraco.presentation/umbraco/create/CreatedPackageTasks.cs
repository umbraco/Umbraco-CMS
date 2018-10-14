using Umbraco.Core.Logging;
using Umbraco.Web.UI;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web._Legacy.UI;
using umbraco.cms.businesslogic.packager;

namespace Umbraco.Web
{
    public class CreatedPackageTasks : LegacyDialogTask
    {

        public override bool PerformSave()
        {
            Current.Logger.Info<CreatedPackageTasks>("Xml save started");
            int id = CreatedPackage.MakeNew(Alias).Data.Id;
            _returnUrl = string.Format("developer/packages/editPackage.aspx?id={0}", id);
            return true;
        }

        public override bool PerformDelete()
        {
            // we need to grab the id from the alias as the new tree needs to prefix the NodeID with "package_"
            if (ParentID == 0)
            {
                ParentID = int.Parse(Alias.Substring("package_".Length));
            }
            CreatedPackage.GetById(ParentID).Delete();
            return true;
        }

        private string _returnUrl = "";

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return Constants.Applications.Packages.ToString(); }
        }
    }
}
