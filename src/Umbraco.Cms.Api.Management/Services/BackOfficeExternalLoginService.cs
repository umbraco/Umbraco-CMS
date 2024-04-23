using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Services;

public class BackOfficeExternalLoginService : IBackOfficeExternalLoginService
{
    private readonly IBackOfficeExternalLoginProviders _backOfficeExternalLoginProviders;
    private readonly IUserService _userService;

    public BackOfficeExternalLoginService(
        IBackOfficeExternalLoginProviders backOfficeExternalLoginProviders,
        IUserService userService)
    {
        _backOfficeExternalLoginProviders = backOfficeExternalLoginProviders;
        _userService = userService;
    }

    public async Task<Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus>>
        ExternalLoginStatusForUser(Guid userid)
    {
        IEnumerable<BackOfficeExternaLoginProviderScheme> providers =
            await _backOfficeExternalLoginProviders.GetBackOfficeProvidersAsync();

        Attempt<ICollection<IIdentityUserLogin>, UserOperationStatus> linkedLoginsAttempt =
            await _userService.GetLinkedLoginsAsync(userid);
        if (linkedLoginsAttempt.Success is false)
        {
            return Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus>.Fail(
                FromUserOperationStatusFailure(linkedLoginsAttempt.Status),
                Enumerable.Empty<UserExternalLoginProviderModel>());
        }

        IEnumerable<UserExternalLoginProviderModel> providerStatuses = providers.Select(
            providerScheme => new UserExternalLoginProviderModel(
                providerScheme.ExternalLoginProvider.AuthenticationType,
                linkedLoginsAttempt.Result.Any(linkedLogin =>
                    linkedLogin.LoginProvider == providerScheme.ExternalLoginProvider.AuthenticationType),
                providerScheme.ExternalLoginProvider.Options.AutoLinkOptions.AllowManualLinking));

        return Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus>.Succeed(
                ExternalLoginOperationStatus.Success, providerStatuses);
    }

    private ExternalLoginOperationStatus FromUserOperationStatusFailure(UserOperationStatus userOperationStatus)
    {
        return userOperationStatus switch
        {
            UserOperationStatus.MissingUser => ExternalLoginOperationStatus.UserNotFound,
            _ => ExternalLoginOperationStatus.Unknown
        };
    }
}
