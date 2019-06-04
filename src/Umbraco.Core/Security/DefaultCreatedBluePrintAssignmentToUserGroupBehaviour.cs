using System;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Core.Security
{
    public class DefaultCreatedBluePrintAssignmentToUserGroupBehaviour : ICreatedBluePrintAssignmentToUserGroupBehaviour
    {
        private readonly IContentService _contentService;

        public DefaultCreatedBluePrintAssignmentToUserGroupBehaviour(IContentService contentService)
        {
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        }

        public void AssignUserGroupsToBlueprint(IContent blueprint, IUser currentUser)
        {
            if (ShouldAssignUserGroupsToBlueprint(currentUser))
            {
                _contentService.AssignGroupsToBlueprintById(
                    blueprint.Id,
                    currentUser.Groups
                        .Where(x => x.StartContentId.HasValue && x.StartContentId != Constants.System.Root)
                        .Select(x => x.Id)
                        .ToArray());
            }
        }

        private static bool ShouldAssignUserGroupsToBlueprint(IUser currentUser)
        {
            return !currentUser.IsAdmin() &&
                   currentUser.StartContentIds != null &&
                   !currentUser.StartContentIds.Contains(Constants.System.Root);
        }
    }
}
