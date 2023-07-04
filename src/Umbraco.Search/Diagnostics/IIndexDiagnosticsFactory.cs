namespace Umbraco.Search.Diagnostics;

/// <summary>
///     Creates <see cref="IIndexDiagnostics" /> for an index if it doesn't implement <see cref="IIndexDiagnostics" />
/// </summary>
public interface IIndexDiagnosticsFactory
{
    IIndexDiagnostics Create(string index);
}
