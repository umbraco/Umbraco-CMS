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
    //This was only referenced from the tree : loadMember which is trying to load this node type alias
    // to show pages that are owned by members but this tree is also trying to show an editor that doesn't exist
    // and I've never even heard of this functionality and think it was an idea that never got completed.
    // I'm nearly positive this is not used and should be un-referenced.
    [Obsolete("This class is no longer used and will be removed from the codebase in future versions")]
    public class contentItemTasks : LegacyDialogTask
    {
        
        public override bool PerformSave()
        {
            // TODO : fix it!!
            return true;
        }

        public override bool PerformDelete()
        {
            var d = new cms.businesslogic.contentitem.ContentItem(ParentID);

            // Version3.0 - moving to recycle bin instead of deletion
            //d.delete();
            d.Move(-20);
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
