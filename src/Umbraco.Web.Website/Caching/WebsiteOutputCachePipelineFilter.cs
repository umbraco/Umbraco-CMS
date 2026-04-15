using Microsoft.AspNetCore.Builder;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Pipeline filter that registers the ASP.NET Core output cache middleware.
/// </summary>
internal sealed class WebsiteOutputCachePipelineFilter : UmbracoPipelineFilter
{
    public WebsiteOutputCachePipelineFilter(string name)
        : base(name)
        => PostPipeline = PostPipelineAction;

    private void PostPipelineAction(IApplicationBuilder applicationBuilder)
        => applicationBuilder.UseOutputCache();
}
