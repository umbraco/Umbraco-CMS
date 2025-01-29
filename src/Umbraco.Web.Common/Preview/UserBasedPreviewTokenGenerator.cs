using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Web.Common.Preview;

public class UserBasedPreviewTokenGenerator : IPreviewTokenGenerator
{
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IUserService _userService;
    private readonly ILogger<UserBasedPreviewTokenGenerator> _logger;

    public UserBasedPreviewTokenGenerator(
        IDataProtectionProvider dataProtectionProvider,
        IUserService userService,
        ILogger<UserBasedPreviewTokenGenerator> logger)
    {
        _dataProtectionProvider = dataProtectionProvider;
        _userService = userService;
        _logger = logger;
    }

    public Task<Attempt<string?>> GenerateTokenAsync(Guid userKey)
    {
        var token = EncryptionHelper.Encrypt(userKey.ToString(), _dataProtectionProvider);

        return Task.FromResult(Attempt.Succeed(token));
    }

    public async Task<Attempt<Guid?>> VerifyAsync(string token)
    {
        try
        {
            var decrypted = EncryptionHelper.Decrypt(token, _dataProtectionProvider);
            if (Guid.TryParse(decrypted, out Guid key))
            {
                IUser? user = await _userService.GetAsync(key);

                if (user is { IsApproved: true, IsLockedOut: false })
                {
                    return Attempt.Succeed<Guid?>(user.Key);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "An error occured when trying to get the user from the encrypted token");
        }

        return Attempt.Fail<Guid?>(null);
    }
}
