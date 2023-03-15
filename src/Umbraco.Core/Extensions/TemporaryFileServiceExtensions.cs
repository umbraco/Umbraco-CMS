using System.Runtime.CompilerServices;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

public static class TemporaryFileServiceExtensions
{
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
