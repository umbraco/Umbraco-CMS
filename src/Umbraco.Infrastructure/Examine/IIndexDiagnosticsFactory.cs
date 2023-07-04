using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Creates <see cref="IIndexDiagnostics" /> for an index if it doesn't implement <see cref="IIndexDiagnostics" />
/// </summary>
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]

public interface IIndexDiagnosticsFactory
{
    IIndexDiagnostics Create(IIndex index);
}
