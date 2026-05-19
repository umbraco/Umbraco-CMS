using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
#if IncludeExample
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Extension.ViewModels;
#endif

namespace Umbraco.Extension.Controllers
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "Umbraco.Extension")]
    public class UmbracoExtensionApiController : UmbracoExtensionApiControllerBase
    {
#if IncludeExample
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        public UmbracoExtensionApiController(IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        }
#endif

        [HttpGet("ping")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        public string Ping() => "Pong";
#if IncludeExample

        [HttpGet("whatsTheTimeMrWolf")]
        [ProducesResponseType(typeof(DateTime), 200)]
        public DateTime WhatsTheTimeMrWolf() => DateTime.Now;

        [HttpGet("whatsMyName")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        public string WhatsMyName()
        {
            // So we can see a long request in the dashboard with a spinning progress wheel
            Thread.Sleep(2000);

            IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
            return currentUser?.Name ?? "I have no idea who you are";
        }

        [HttpGet("whoAmI")]
        [ProducesResponseType<WhoAmIResponseModel>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<WhoAmIResponseModel> WhoAmI()
        {
            IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
            if (currentUser is null)
            {
                return NoContent();
            }

            return new WhoAmIResponseModel
            {
                Name = currentUser.Name,
                Email = currentUser.Email,
                Groups = currentUser.Groups
                    .Select(group => group.Name)
                    .OfType<string>()
                    .ToArray(),
            };
        }
#endif
    }
}
