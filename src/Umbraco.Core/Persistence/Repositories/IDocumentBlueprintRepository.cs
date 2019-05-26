using System.Collections.Generic;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IDocumentBlueprintRepository : IDocumentRepository
    {
        /// <summary>
        /// Retrieves the user groups that have been assigned to the blueprint
        /// </summary>
        /// <param name="id">Blueprint node Id</param>
        IEnumerable<IUserGroup> GetGroupsAssignedToBlueprint(int id);

        /// <summary>
        /// Assigns the set of uer groups to the blueprint
        /// </summary>
        /// <param name="id">Blueprint node Id</param>
        /// <param name="userGroupIds">User group Ids</param>
        void AssignGroupsToBlueprint(int id, int[] userGroupIds);
    }
}
