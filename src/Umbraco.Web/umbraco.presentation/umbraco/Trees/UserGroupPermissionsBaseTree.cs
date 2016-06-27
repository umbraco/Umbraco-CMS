using umbraco.businesslogic;
using umbraco.interfaces;
using System.Collections.Generic;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;

namespace umbraco.cms.presentation.Trees
{
    [Tree(Constants.Applications.Users, "userPermissions", "User Permissions", sortOrder: 3)]
    public abstract class UserGroupPermissionsBaseTree : BaseTree
    {
        protected UserGroupPermissionsBaseTree(string application) : base(application) { }


        /// <summary>
        /// don't allow any actions on this tree
        /// </summary>
        /// <param name="actions"></param>
        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
        }

        /// <summary>
        /// no actions should be able to be performed on the parent node except for refresh
        /// </summary>
        /// <param name="actions"></param>
        protected override void CreateRootNodeActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionRefresh.Instance);
        }
    }
}
