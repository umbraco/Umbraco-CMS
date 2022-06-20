using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Creates <see cref="IIndexDiagnostics" /> for an index if it doesn't implement <see cref="IIndexDiagnostics" />
/// </summary>
public interface IIndexDiagnosticsFactory
{
    IIndexDiagnostics Create(IIndex index);
}
