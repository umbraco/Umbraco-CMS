using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class WebProfilerService : IWebProfilerService
{
    private readonly IWebProfilerRepository _webProfilerRepository;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebProfilerService" /> class.
    /// </summary>
    /// <param name="webProfilerRepository">The web profiler repository.</param>
    /// <param name="backOfficeSecurityAccessor">The backoffice security accessor.</param>
    public WebProfilerService(IWebProfilerRepository webProfilerRepository, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _webProfilerRepository = webProfilerRepository;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <inheritdoc />
    public Task<Attempt<bool, WebProfilerOperationStatus>> GetStatus()
    {
        Attempt<int> userIdAttempt = GetExecutingUserId();

        if (userIdAttempt.Success is false)
        {
            return Task.FromResult(Attempt.FailWithStatus(WebProfilerOperationStatus.ExecutingUserNotFound, false));
        }

        var result = _webProfilerRepository.GetStatus(userIdAttempt.Result);
        return Task.FromResult(Attempt.SucceedWithStatus(WebProfilerOperationStatus.Success, result));
    }

    /// <inheritdoc />
    public Task<Attempt<bool, WebProfilerOperationStatus>> SetStatus(bool status)
    {
        Attempt<int> userIdAttempt = GetExecutingUserId();

        if (userIdAttempt.Success is false)
        {
            return Task.FromResult(Attempt.FailWithStatus(WebProfilerOperationStatus.ExecutingUserNotFound, false));
        }

        _webProfilerRepository.SetStatus(userIdAttempt.Result, status);
        return Task.FromResult(Attempt.SucceedWithStatus(WebProfilerOperationStatus.Success, status));
    }

    /// <summary>
    ///     Gets the identifier of the currently executing user.
    /// </summary>
    /// <returns>An <see cref="Attempt{T}" /> containing the user identifier if successful.</returns>
    private Attempt<int> GetExecutingUserId()
    {
        //FIXME when we can get current user
        return Attempt.Succeed(-1);
    }
}
