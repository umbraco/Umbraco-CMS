using System;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Compose
{
    public sealed class PublicAccessComponent : IComponent
    {
        private readonly IPublicAccessService _publicAccessService;
        public PublicAccessComponent(IPublicAccessService publicAccessService)
        {
            _publicAccessService = publicAccessService ?? throw new ArgumentNullException(nameof(publicAccessService));
        }

        public void Initialize()
        {
            MemberGroupService.Saved += MemberGroupService_Saved;
        }

        public void Terminate()
        {
            MemberGroupService.Saved -= MemberGroupService_Saved;
        }

        private void MemberGroupService_Saved(IMemberGroupService sender, SaveEventArgs<IMemberGroup> e)
        {
            foreach (var grp in e.SavedEntities)
            {
                //check if the name has changed
                if (grp.AdditionalData.ContainsKey("previousName")
                    && grp.AdditionalData["previousName"] != null
                    && grp.AdditionalData["previousName"].ToString().IsNullOrWhiteSpace() == false
                    && grp.AdditionalData["previousName"].ToString() != grp.Name)
                {
                    _publicAccessService.RenameMemberGroupRoleRules(grp.AdditionalData["previousName"].ToString(), grp.Name);
                }
            }
        }
    }
}
