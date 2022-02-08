using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.UI.Security
{
    public class SecurityComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {

            builder.AddExternalAuth();

            var identityBuilder = new MemberIdentityBuilder(builder.Services);

            identityBuilder.AddTwoFactorProvider<UmbracoAppAuthenticator>(UmbracoAppAuthenticator.Name);
            identityBuilder.AddTwoFactorProvider<EmailTwoFactorMemberProvider>(EmailTwoFactorMemberProvider.Name);


            builder
                .AddNotificationHandler<MemberTwoFactorRequestedNotification, SendOneTimePasswordNotificationHandler>();
        }
    }
}
