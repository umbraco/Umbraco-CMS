namespace Umbraco.Search.Diagnostics;

/// <summary>
///     Creates <see cref="IIndexDiagnostics" /> for an index if it doesn't implement <see cref="IIndexDiagnostics" />
/// </summary>
public interface IIndexDiagnosticsFactory<T>
{
    IIndexDiagnostics<T> Create<T>(string index);
}
