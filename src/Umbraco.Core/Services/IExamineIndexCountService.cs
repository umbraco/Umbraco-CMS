namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides functionality to retrieve counts from Examine search indexes.
/// </summary>
public interface IExamineIndexCountService
{
    /// <summary>
    /// Gets the total number of documents across all Examine indexes.
    /// </summary>
    /// <returns>The total count of indexed documents.</returns>
    public int GetCount();
}
