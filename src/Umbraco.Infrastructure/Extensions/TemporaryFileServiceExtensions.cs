using System.Runtime.CompilerServices;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

public static class TemporaryFileServiceExtensions
{
    public static void EnlistDeleteIfScopeCompletes(this ITemporaryFileService temporaryFileService, Guid temporaryFileKey, IScopeProvider scopeProvider, [CallerMemberName] string memberName = "")
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
