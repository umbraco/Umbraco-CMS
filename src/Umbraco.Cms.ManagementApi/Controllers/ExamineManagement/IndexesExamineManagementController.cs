using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.ExamineManagement;

public class IndexesExamineManagementController : ExamineManagementControllerBase
{
    private readonly IExamineManager _examineManager;
    private readonly IExamineIndexModelFactory _examineIndexModelFactory;
    public IndexesExamineManagementController(
        IExamineManager examineManager,
        IExamineIndexModelFactory examineIndexModelFactory)
    {
        _examineManager = examineManager;
        _examineIndexModelFactory = examineIndexModelFactory;
    }

    /// <summary>
    ///     Get the details for indexers
    /// </summary>
    /// <returns></returns>
    public PagedViewModel<ExamineIndexModel> GetIndexerDetails(int skip, int take)
    {
        ExamineIndexModel[] indexes = _examineManager.Indexes
            .Select(_examineIndexModelFactory.Create)
            .OrderBy(examineIndexModel => examineIndexModel.Name?.TrimEnd("Indexer")).ToArray();

        return new PagedViewModel<ExamineIndexModel> { Items = indexes.Skip(skip).Take(take), Total = indexes.Length };
    }
}
