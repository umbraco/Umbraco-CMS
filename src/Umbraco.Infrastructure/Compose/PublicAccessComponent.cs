using System;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Web.Compose
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
            MemberGroupService.Saved += (s, e) => MemberGroupService_Saved(s, e, _publicAccessService);
        }

        public void Terminate()
        { }

        static void MemberGroupService_Saved(IMemberGroupService sender, Core.Events.SaveEventArgs<Core.Models.IMemberGroup> e, IPublicAccessService publicAccessService)
        {
            foreach (var grp in e.SavedEntities)
            {
                //check if the name has changed
                if (grp.AdditionalData.ContainsKey("previousName")
                    && grp.AdditionalData["previousName"] != null
                    && grp.AdditionalData["previousName"].ToString().IsNullOrWhiteSpace() == false
                    && grp.AdditionalData["previousName"].ToString() != grp.Name)
                {
                    publicAccessService.RenameMemberGroupRoleRules(grp.AdditionalData["previousName"].ToString(), grp.Name);
                }
            }
        }
    }
}
