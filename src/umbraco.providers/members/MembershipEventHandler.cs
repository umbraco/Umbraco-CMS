using System.Web.Security;
using Umbraco.Core;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using Umbraco.Core.Security;

namespace umbraco.providers.members
{
    /// <summary>
    /// Adds some event handling
    /// </summary>
    public class MembershipEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            Member.New += Member_New;
        }

        void Member_New(Member sender, NewEventArgs e)
        {
            //This is a bit of a hack to ensure that the member is approved when created since many people will be using
            // this old api to create members on the front-end and they need to be approved - which is based on whether or not 
            // the Umbraco membership provider is configured.
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider() as UmbracoMembershipProvider;
            if (provider != null)
            {
                var approvedField = provider.ApprovedPropertyTypeAlias;
                var property = sender.getProperty(approvedField);
                if (property != null)
                {
                    property.Value = 1;
                    sender.Save();
                }
            }            
        }
    }
}