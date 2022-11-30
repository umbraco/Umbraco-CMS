using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.Security;

public class NoopBackOfficeUserPasswordChecker : IBackOfficeUserPasswordChecker
{
    public Task<BackOfficeUserPasswordCheckerResult> CheckPasswordAsync(BackOfficeIdentityUser user, string password)
        => Task.FromResult(BackOfficeUserPasswordCheckerResult.FallbackToDefaultChecker);
}
