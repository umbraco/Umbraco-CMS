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
    //This was only referenced from the obsolete tree that is not used: loadcontentItemType which is trying to load
    // pages that don't even exist so I'm nearly positive this isn't used and should be removed.
    [Obsolete("This class is no longer used and will be removed from the codebase in future versions")]
    public class contentItemTypeTasks : LegacyDialogTask
    {
        public override bool PerformSave()
        {

            cms.businesslogic.contentitem.ContentItemType.MakeNew(User, Alias);
            return true;
        }

        public override bool PerformDelete()
        {
            new cms.businesslogic.contentitem.ContentItemType(ParentID).delete();
            return true;
        }

        public override string ReturnUrl
        {
            get { return string.Empty; }
        }

        public override string AssignedApp
        {
            get { return DefaultApps.member.ToString(); }
        }
    }
}
