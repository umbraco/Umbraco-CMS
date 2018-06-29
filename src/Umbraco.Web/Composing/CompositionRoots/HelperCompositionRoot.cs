using System.Web.Security;
using LightInject;
using Umbraco.Web.Security;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Web.Composing.CompositionRoots
{
    public class HelperCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry container)
        {
            container.Register((factory) => Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider());
            container.Register((factory) => Roles.Enabled ? Roles.Provider : new MembersRoleProvider(Core.Composing.Current.Services.MemberService));
            container.Register<MembershipHelper>();
        }
    }
}
