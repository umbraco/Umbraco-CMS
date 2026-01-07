using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.LogViewer;

/// <summary>
/// Base class for LogViewer integration tests that ensures the log directory exists.
/// </summary>
public abstract class LogViewerTestBase<T> : ManagementApiUserGroupTestBase<T>
    where T : ManagementApiControllerBase
{
    [SetUp]
    public void EnsureLogDirectoryExists()
    {
        var loggingConfiguration = GetRequiredService<ILoggingConfiguration>();
        if (!Directory.Exists(loggingConfiguration.LogDirectory))
        {
            Directory.CreateDirectory(loggingConfiguration.LogDirectory);
        }
    }
}
