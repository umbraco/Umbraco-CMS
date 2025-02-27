using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IWebProfilerService
{
    Task<Attempt<bool, WebProfilerOperationStatus>> GetStatus();
    Task<Attempt<bool, WebProfilerOperationStatus>> SetStatus(bool status);
}
