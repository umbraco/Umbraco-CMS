using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security
{
    // TODO: This has been migrated to netcore
    public class BackOfficeSignInManager : IDisposable
    {
        private readonly IBackOfficeUserManager _userManager;
        private readonly IUserClaimsPrincipalFactory<BackOfficeIdentityUser> _claimsPrincipalFactory;
        private readonly IAuthenticationManager _authenticationManager;
        private readonly ILogger _logger;
        private readonly GlobalSettings _globalSettings;
        private readonly IOwinRequest _request;

        public BackOfficeSignInManager(
            IBackOfficeUserManager userManager,
            IUserClaimsPrincipalFactory<BackOfficeIdentityUser> claimsPrincipalFactory,
            IAuthenticationManager authenticationManager,
            ILogger logger,
            GlobalSettings globalSettings,
            IOwinRequest request)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _claimsPrincipalFactory = claimsPrincipalFactory ?? throw new ArgumentNullException(nameof(claimsPrincipalFactory));
            _authenticationManager = authenticationManager ?? throw new ArgumentNullException(nameof(authenticationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public void Dispose()
        {
        }
    }
}
