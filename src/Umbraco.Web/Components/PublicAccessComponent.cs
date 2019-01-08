using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Components
{
    public sealed class PublicAccessComponent : IComponent
    {
        public void Initialize()
        {
            MemberGroupService.Saved += MemberGroupService_Saved;
        }

        public void Terminate()
        { }

        static void MemberGroupService_Saved(IMemberGroupService sender, Core.Events.SaveEventArgs<Core.Models.IMemberGroup> e)
        {
            foreach (var grp in e.SavedEntities)
            {
                //check if the name has changed
                if (grp.AdditionalData.ContainsKey("previousName")
                    && grp.AdditionalData["previousName"] != null
                    && grp.AdditionalData["previousName"].ToString().IsNullOrWhiteSpace() == false
                    && grp.AdditionalData["previousName"].ToString() != grp.Name)
                {
                    Current.Services.PublicAccessService.RenameMemberGroupRoleRules(grp.AdditionalData["previousName"].ToString(), grp.Name);
                }
            }
        }
    }
}
