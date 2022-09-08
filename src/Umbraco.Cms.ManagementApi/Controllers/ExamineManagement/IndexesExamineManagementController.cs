using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Server;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.ExamineManagement;

[ApiVersion("1.0")]
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
    [HttpGet("Indexes")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerStatusViewModel), StatusCodes.Status200OK)]
    public PagedViewModel<ExamineIndexModel> Indexes(int skip, int take)
    {
        ExamineIndexModel[] indexes = _examineManager.Indexes
            .Select(_examineIndexModelFactory.Create)
            .OrderBy(examineIndexModel => examineIndexModel.Name?.TrimEnd("Indexer")).ToArray();

        return new PagedViewModel<ExamineIndexModel> { Items = indexes.Skip(skip).Take(take), Total = indexes.Length };
    }
}
