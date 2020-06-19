using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Owin;
using Umbraco.Core;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security
{

    /// <summary>
    /// This is used to inspect the request to see if 2 x identities are assigned: A windows one and a back office one.
    /// When this is the case, it means that auth has executed for Windows & auth has executed for our back office cookie
    /// handler and now two identities have been assigned. Unfortunately, at some stage in the pipeline - I'm pretty sure
    /// it's the Role Provider Module - it again changes the user's Principal to a RolePrincipal and discards the second
    /// Identity which is the Back office identity thus preventing a user from accessing the back office... it's very annoying.
    ///
    /// To fix this, we re-set the user Principal to only have a single identity: the back office one, since we know this is
    /// for a back office request.
    /// </summary>
    internal class FixWindowsAuthMiddlware : OwinMiddleware
    {
        public FixWindowsAuthMiddlware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (context.Request.Uri.IsClientSideRequest() == false)
            {
                var claimsPrincipal = context.Request.User as ClaimsPrincipal;
                if (claimsPrincipal != null
                    && claimsPrincipal.Identities.Count() > 1
                    && claimsPrincipal.Identities.Any(x => x is WindowsIdentity)
                    && claimsPrincipal.Identities.Any(x => x is UmbracoBackOfficeIdentity))
                {
                    var backOfficeIdentity = claimsPrincipal.Identities.First(x => x is UmbracoBackOfficeIdentity);
                    if (backOfficeIdentity.IsAuthenticated)
                    {
                        context.Request.User = new ClaimsPrincipal(backOfficeIdentity);
                    }
                }
            }

            if (Next != null)
            {
                await Next.Invoke(context);
            }
        }
    }
}
