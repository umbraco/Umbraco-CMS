using System.Runtime.CompilerServices;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ITemporaryFileService"/>.
/// </summary>
public static class TemporaryFileServiceExtensions
{
    /// <summary>
    /// Enlists a temporary file for deletion when the current scope completes successfully.
    /// </summary>
    /// <param name="temporaryFileService">The temporary file service.</param>
    /// <param name="temporaryFileKey">The key of the temporary file to delete.</param>
    /// <param name="scopeProvider">The core scope provider.</param>
    /// <param name="memberName">The name of the calling member (automatically populated).</param>
    public static void EnlistDeleteIfScopeCompletes(this ITemporaryFileService temporaryFileService, Guid temporaryFileKey, ICoreScopeProvider scopeProvider, [CallerMemberName] string memberName = "")
    {
        scopeProvider.Context?.Enlist(
            temporaryFileKey.ToString(),
            () => memberName,
            (completed, svc) =>
            {
                if (completed)
                {
                    temporaryFileService.DeleteAsync(temporaryFileKey);
                }
            });
    }
}
