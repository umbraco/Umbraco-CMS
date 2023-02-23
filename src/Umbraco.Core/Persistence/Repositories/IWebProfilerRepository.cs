namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IWebProfilerRepository
{
    void SetStatus(int userId, bool status);
    bool GetStatus(int userId);
}
