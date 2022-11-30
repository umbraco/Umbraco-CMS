using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.ContentApi.Filters;
using Umbraco.Cms.ContentApi.Routing;
using Umbraco.Cms.Core.Headless;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.ContentApi.Controllers;

[ContentApiJsonConfiguration]
[VersionedContentApiRoute("query")]
[ApiVersion("1.0")]
public class ContentApiController : Controller
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IHeadlessContentBuilder _headlessContentBuilder;

    public ContentApiController(IPublishedSnapshotAccessor publishedSnapshotAccessor, IHeadlessContentBuilder headlessContentBuilder)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _headlessContentBuilder = headlessContentBuilder;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IHeadlessContent>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    // TODO: actually implement the content API, this is just to test headless output rendering
    public async Task<IActionResult> Get(int skip = 0, int take = 100)
    {
        IPublishedContentCache? contentCache = _publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? publishedSnapshot)
                ? publishedSnapshot?.Content
                : null;
        if (contentCache == null)
        {
            // TODO: refactor - move ProblemDetailsBuilder to Common and reuse it here
            return BadRequest(
                new ProblemDetailsBuilder()
                    .WithTitle("Content cache is not available")
                    .WithDetail("Could not retrieve the content cache. Umbraco may be in an error state.")
                    .Build());
        }

        IHeadlessContent[] result = contentCache
            .GetAtRoot()
            .Select(_headlessContentBuilder.Build)
            .ToArray();

        return await Task.FromResult(
            Ok(
                new PagedViewModel<IHeadlessContent>
                {
                    Items = result.Skip(skip).Take(take).ToArray(),
                    Total = result.Length
                }));
    }
}

// TODO: refactor - move PagedViewModel from Management API to Common and delete this one
public class PagedViewModel<T>
{
    [Required]
    public long Total { get; set; }

    [Required]
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    public static PagedViewModel<T> Empty() => new();
}

// TODO: refactor - move ProblemDetailsBuilder from Management API to Common and delete this one
public class ProblemDetailsBuilder
{
    private string? _title;
    private string? _detail;
    private int _status = StatusCodes.Status400BadRequest;
    private string? _type;

    public ProblemDetailsBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ProblemDetailsBuilder WithDetail(string detail)
    {
        _detail = detail;
        return this;
    }

    public ProblemDetailsBuilder WithStatus(int status)
    {
        _status = status;
        return this;
    }

    public ProblemDetailsBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public ProblemDetails Build() =>
        new()
        {
            Title = _title,
            Detail = _detail,
            Status = _status,
            Type = _type ?? "Error",
        };
}
