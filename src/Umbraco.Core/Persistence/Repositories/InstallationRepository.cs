using System.Text;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Provides an implementation of <see cref="IInstallationRepository" /> for installation logging.
/// </summary>
[Obsolete("Installation logging is no longer supported and this class will be removed in Umbraco 19.")]
public class InstallationRepository : IInstallationRepository
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallationRepository" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public InstallationRepository(IJsonSerializer jsonSerializer)
    {

    }

    /// <inheritdoc />
    [Obsolete("This method no longer has any function and will be removed in Umbraco 19.")]
    public Task SaveInstallLogAsync(InstallLog installLog) => Task.CompletedTask;
}
