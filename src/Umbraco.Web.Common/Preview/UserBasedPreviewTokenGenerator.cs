using Microsoft.AspNetCore.DataProtection;
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

    public UserBasedPreviewTokenGenerator(IDataProtectionProvider dataProtectionProvider, IUserService userService)
    {
        _dataProtectionProvider = dataProtectionProvider;
        _userService = userService;
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
        catch
        {
            // ignored
        }

        return Attempt.Fail<Guid?>(null);
    }
}
