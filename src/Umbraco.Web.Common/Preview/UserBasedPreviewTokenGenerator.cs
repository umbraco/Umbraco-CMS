using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.Preview;

/// <summary>
///     Generates and verifies preview tokens identifying a back-office user.
/// </summary>
/// <remarks>
///     A token expires after the back-office login timeout (<see cref="GlobalSettings.TimeOut" />), and is bound to the
///     user's security stamp so that it no longer verifies once the user's credentials change.
/// </remarks>
[Obsolete("This has been deprecated and will be removed in Umbraco 19.")]
public class UserBasedPreviewTokenGenerator : IPreviewTokenGenerator
{
    private const string ProtectorPurpose = "Umbraco.Cms.Web.Common.Preview.PreviewToken";
    private const char PayloadSeparator = '|';

    private readonly IUserService _userService;
    private readonly ILogger<UserBasedPreviewTokenGenerator> _logger;
    private readonly ITimeLimitedDataProtector _dataProtector;
    private readonly TimeSpan _lifetime;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserBasedPreviewTokenGenerator" /> class.
    /// </summary>
    /// <param name="dataProtectionProvider">The data protection provider used to protect the tokens.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="globalSettings">The global settings, providing the token lifetime.</param>
    public UserBasedPreviewTokenGenerator(
        IDataProtectionProvider dataProtectionProvider,
        IUserService userService,
        ILogger<UserBasedPreviewTokenGenerator> logger,
        IOptions<GlobalSettings> globalSettings)
    {
        _userService = userService;
        _logger = logger;
        _dataProtector = dataProtectionProvider.CreateProtector(ProtectorPurpose).ToTimeLimitedDataProtector();
        _lifetime = globalSettings.Value.TimeOut;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserBasedPreviewTokenGenerator" /> class.
    /// </summary>
    /// <param name="dataProtectionProvider">The data protection provider used to protect the tokens.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="logger">The logger.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public UserBasedPreviewTokenGenerator(
        IDataProtectionProvider dataProtectionProvider,
        IUserService userService,
        ILogger<UserBasedPreviewTokenGenerator> logger)
        : this(
            dataProtectionProvider,
            userService,
            logger,
            StaticServiceProvider.Instance.GetRequiredService<IOptions<GlobalSettings>>())
    {
    }

    /// <inheritdoc />
    public async Task<Attempt<string?>> GenerateTokenAsync(Guid userKey)
    {
        IUser? user = await _userService.GetAsync(userKey);
        if (user is null)
        {
            return Attempt.Fail<string?>(null);
        }

        var payload = $"{user.Key:D}{PayloadSeparator}{user.SecurityStamp}";
        var token = _dataProtector.Protect(payload, _lifetime);

        return Attempt.Succeed<string?>(token);
    }

    /// <inheritdoc />
    public async Task<Attempt<Guid?>> VerifyAsync(string token)
    {
        try
        {
            var payload = _dataProtector.Unprotect(token);
            var separatorIndex = payload.IndexOf(PayloadSeparator);
            if (separatorIndex > 0 && Guid.TryParse(payload[..separatorIndex], out Guid key))
            {
                var securityStamp = payload[(separatorIndex + 1)..];
                IUser? user = await _userService.GetAsync(key);

                if (user is { IsApproved: true, IsLockedOut: false }
                    && string.Equals(securityStamp, user.SecurityStamp ?? string.Empty, StringComparison.Ordinal))
                {
                    return Attempt.Succeed<Guid?>(user.Key);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "An error occurred when trying to get the user from the preview token");
        }

        return Attempt.Fail<Guid?>(null);
    }
}
