using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal sealed class WebProfilerService : IWebProfilerService
{
    private readonly IWebProfilerRepository _webProfilerRepository;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public WebProfilerService(IWebProfilerRepository webProfilerRepository, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _webProfilerRepository = webProfilerRepository;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    public async Task<Attempt<bool, WebProfilerOperationStatus>> GetStatus()
    {
        Attempt<int> userIdAttempt = GetExecutingUserId();

        if (userIdAttempt.Success is false)
        {
            return Attempt.FailWithStatus(WebProfilerOperationStatus.ExecutingUserNotFound, false);
        }

        var result = _webProfilerRepository.GetStatus(userIdAttempt.Result);
        return await Task.FromResult(Attempt.SucceedWithStatus(WebProfilerOperationStatus.Success, result));
    }

    public async Task<Attempt<bool, WebProfilerOperationStatus>> SetStatus(bool status)
    {
        Attempt<int> userIdAttempt = GetExecutingUserId();

        if (userIdAttempt.Success is false)
        {
            return Attempt.FailWithStatus(WebProfilerOperationStatus.ExecutingUserNotFound, false);
        }

        _webProfilerRepository.SetStatus(userIdAttempt.Result, status);
        return await Task.FromResult(Attempt.SucceedWithStatus(WebProfilerOperationStatus.Success, status));
    }

    private Attempt<int> GetExecutingUserId()
    {
        //FIXME when we can get current user
        return Attempt.Succeed(-1);

#pragma warning disable CS0162 // Unreachable code detected
#pragma warning disable CS0618 // Type or member is obsolete
        Attempt<int>? userIdAttempt = _backOfficeSecurityAccessor?.BackOfficeSecurity?.GetUserId();
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0162 // Unreachable code detected

        return (userIdAttempt.HasValue && userIdAttempt.Value.Success)
            ? Attempt.Succeed(userIdAttempt.Value.Result)
            : Attempt.Fail(0);
    }
}
