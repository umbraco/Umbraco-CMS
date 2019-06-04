using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Security
{
    public interface ICreatedBluePrintAssignmentToUserGroupBehaviour
    {
        void AssignUserGroupsToBlueprint(IContent blueprint, IUser currentUser);
    }
}
