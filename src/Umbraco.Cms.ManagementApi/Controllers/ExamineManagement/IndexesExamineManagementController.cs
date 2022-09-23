using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.ExamineManagement;

[ApiVersion("1.0")]
public class IndexesExamineManagementController : ExamineManagementControllerBase
{
    private readonly IExamineManager _examineManager;
    private readonly IExamineIndexViewModelFactory _examineIndexViewModelFactory;

    public IndexesExamineManagementController(
        IExamineManager examineManager,
        IExamineIndexViewModelFactory examineIndexViewModelFactory)
    {
        _examineManager = examineManager;
        _examineIndexViewModelFactory = examineIndexViewModelFactory;
    }

    /// <summary>
    ///     Get the details for indexers
    /// </summary>
    /// <returns></returns>
    [HttpGet("indexes")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ExamineIndexModel>), StatusCodes.Status200OK)]
    // This endpoint for now will throw errors if the ExamineIndexViewModel ever has providerProperties defined
    // This is because System.Text.Json cannot serialize dictionary<string, object>
    // This has been fixed in .NET 7, so this will work when we upgrade: https://github.com/dotnet/runtime/issues/67588
    public async Task<PagedViewModel<ExamineIndexViewModel>> Indexes(int skip, int take)
    {
        ExamineIndexViewModel[] indexes = _examineManager.Indexes
            .Select(_examineIndexViewModelFactory.Create)
            .OrderBy(examineIndexModel => examineIndexModel.Name?.TrimEnd("Indexer")).ToArray();

        var viewModel = new PagedViewModel<ExamineIndexViewModel> { Items = indexes.Skip(skip).Take(take), Total = indexes.Length };
        return viewModel;
    }
}
