using Microsoft.AspNetCore.Builder;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

/// <summary>
///     Used to modify the <see cref="IApplicationBuilder" /> pipeline before and after Umbraco registers it's core
///     middlewares.
/// </summary>
/// <remarks>
///     Mainly used for package developers.
/// </remarks>
public class UmbracoPipelineFilter : IUmbracoPipelineFilter
{
    public UmbracoPipelineFilter(string name)
        : this(name, null, null, null)
    {
    }

    public UmbracoPipelineFilter(
        string name,
        Action<IApplicationBuilder>? prePipeline,
        Action<IApplicationBuilder>? postPipeline,
        Action<IApplicationBuilder>? preEndpointCallback,
        Action<IApplicationBuilder>? postEndpointCallback = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        PrePipeline = prePipeline;
        PostPipeline = postPipeline;
        PreEndpoints = preEndpointCallback;
        PostEndpoints = postEndpointCallback;
    }

    public Action<IApplicationBuilder>? PrePipeline { get; set; }

    public Action<IApplicationBuilder>? PostPipeline { get; set; }

    // This has been retained for backward compatability
    [Obsolete("Use PreEndpoints for adding endpoints before the Umbraco endpoints and PostEndpoints for endpoints after the Umbraco endpoints")]
    public Action<IApplicationBuilder>? Endpoints
    {
        get
        {
            return PreEndpoints;
        }
        set
        {
            PreEndpoints = value;
        }
    }

    public Action<IApplicationBuilder>? PreEndpoints { get; set; }

    public Action<IApplicationBuilder>? PostEndpoints { get; set; }

    public string Name { get; }

    public void OnPrePipeline(IApplicationBuilder app) => PrePipeline?.Invoke(app);

    public void OnPostPipeline(IApplicationBuilder app) => PostPipeline?.Invoke(app);

    public void OnPreEndpoints(IApplicationBuilder app) => PreEndpoints?.Invoke(app);

    public void OnPostEndpoints(IApplicationBuilder app) => PostEndpoints?.Invoke(app);
}
