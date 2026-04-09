using Examine;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

/// <summary>
/// Provides functionality for counting and retrieving information about items within Examine indexes in Umbraco.
/// </summary>
public class ExamineIndexCountService : IExamineIndexCountService
{
    private readonly IExamineManager _examineManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExamineIndexCountService"/> class, which provides functionality for counting items in Examine indexes.
    /// </summary>
    /// <param name="examineManager">The <see cref="IExamineManager"/> instance used to manage and interact with Examine indexes.</param>
    public ExamineIndexCountService(IExamineManager examineManager) => _examineManager = examineManager;

    /// <summary>
    /// Returns the number of indexes currently managed by the examine manager.
    /// </summary>
    /// <returns>The number of indexes managed by the examine manager.</returns>
    public int GetCount() => _examineManager.Indexes.Count();
}
