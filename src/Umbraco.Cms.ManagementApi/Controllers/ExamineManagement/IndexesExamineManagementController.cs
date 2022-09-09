using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;
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
    [HttpGet("indexes")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ExamineIndexModel>), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<ExamineIndexViewModel>> Indexes(int skip, int take)
    {
        ExamineIndexViewModel[] indexes = _examineManager.Indexes
            .Select(_examineIndexModelFactory.Create)
            .OrderBy(examineIndexModel => examineIndexModel.Name?.TrimEnd("Indexer")).ToArray();

        var viewModel = new PagedViewModel<ExamineIndexViewModel> { Items = indexes.Skip(skip).Take(take), Total = indexes.Length };
        return viewModel;
    }
}
