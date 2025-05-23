using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Tests.Integration.Testing;

public class TestWebProfilerRepository : IWebProfilerRepository
{
    private bool _status = false;

    public void SetStatus(int userId, bool status) => _status = status;

    public bool GetStatus(int userId) => _status;
}
