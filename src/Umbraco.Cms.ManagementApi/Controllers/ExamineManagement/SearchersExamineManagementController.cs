using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.ExamineManagement;

[ApiVersion("1.0")]
public class SearchersExamineManagementController : ExamineManagementControllerBase
{
    private readonly IExamineManager _examineManager;

    public SearchersExamineManagementController(IExamineManager examineManager) => _examineManager = examineManager;

    /// <summary>
    ///     Get the details for searchers
    /// </summary>
    /// <returns></returns>
    [HttpGet("searchers")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ExamineIndexModel>), StatusCodes.Status200OK)]
    // This endpoint for now will throw errors if the ExamineIndexViewModel ever has providerProperties defined
    // This is because System.Text.Json cannot serialize dictionary<string, object>
    // This has been fixed in .NET 7, so this will work when we upgrade: https://github.com/dotnet/runtime/issues/67588
    public async Task<PagedViewModel<ExamineIndexViewModel>> GetSearcherDetails(int skip, int take)
    {
        var searchers = new List<ExamineIndexViewModel>(
            _examineManager.RegisteredSearchers.Select(searcher => new ExamineIndexViewModel { Name = searcher.Name })
                .OrderBy(x =>
                    x.Name?.TrimEnd("Searcher"))); // order by name , but strip the "Searcher" from the end if it exists
        var viewModel = new PagedViewModel<ExamineIndexViewModel>
        {
            Items = searchers.Skip(skip).Take(take),
            Total = searchers.Count,
        };

        return viewModel;
    }
}
