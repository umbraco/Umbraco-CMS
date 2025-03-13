using Microsoft.AspNetCore.Builder;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Cms.Api.Delivery.Caching;

internal sealed class OutputCachePipelineFilter : UmbracoPipelineFilter
{
    public OutputCachePipelineFilter(string name)
        : base(name)
        => PostPipeline = PostPipelineAction;

    private void PostPipelineAction(IApplicationBuilder applicationBuilder)
        => applicationBuilder.UseOutputCache();
}
