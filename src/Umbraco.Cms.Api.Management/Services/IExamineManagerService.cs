using System.Diagnostics.CodeAnalysis;
using Examine;

namespace Umbraco.Cms.Api.Management.Services;

public interface IExamineManagerService
{
    bool TryFindSearcher(
        string searcherName,
        [MaybeNullWhen(false)]
        out ISearcher searcher);
}
