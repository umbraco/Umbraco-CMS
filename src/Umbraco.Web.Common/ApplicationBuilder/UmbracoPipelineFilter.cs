using Microsoft.AspNetCore.Builder;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

/// <inheritdoc />
public class UmbracoPipelineFilter : IUmbracoPipelineFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoPipelineFilter" /> class.
    /// </summary>
    /// <param name="name">The name.</param>
    public UmbracoPipelineFilter(string name)
        : this(name, null, null, null, null, null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoPipelineFilter" /> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="prePipeline">The pre pipeline callback.</param>
    /// <param name="preRouting">The pre routing callback.</param>
    /// <param name="postRouting">The post routing callback.</param>
    /// <param name="postPipeline">The post pipeline callback.</param>
    /// <param name="endpoints">The endpoints callback.</param>
    public UmbracoPipelineFilter(
        string name,
        Action<IApplicationBuilder>? prePipeline = null,
        Action<IApplicationBuilder>? preRouting = null,
        Action<IApplicationBuilder>? postRouting = null,
        Action<IApplicationBuilder>? postPipeline = null,
        Action<IApplicationBuilder>? endpoints = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        PrePipeline = prePipeline;
        PreRouting = preRouting;
        PostRouting = postRouting;
        PostPipeline = postPipeline;
        Endpoints = endpoints;
    }

    /// <summary>
    /// Gets or sets the pre pipeline callback.
    /// </summary>
    /// <value>
    /// The pre pipeline callback.
    /// </value>
    public Action<IApplicationBuilder>? PrePipeline { get; set; }

    /// <summary>
    /// Gets or sets the pre routing.
    /// </summary>
    /// <value>
    /// The pre routing.
    /// </value>
    public Action<IApplicationBuilder>? PreRouting { get; set; }

    /// <summary>
    /// Gets or sets the post routing callback.
    /// </summary>
    /// <value>
    /// The post routing callback.
    /// </value>
    public Action<IApplicationBuilder>? PostRouting { get; set; }

    /// <summary>
    /// Gets or sets the post pipeline callback.
    /// </summary>
    /// <value>
    /// The post pipeline callback.
    /// </value>
    public Action<IApplicationBuilder>? PostPipeline { get; set; }

    /// <summary>
    /// Gets or sets the endpoints callback.
    /// </summary>
    /// <value>
    /// The endpoints callback.
    /// </value>
    public Action<IApplicationBuilder>? Endpoints { get; set; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public void OnPrePipeline(IApplicationBuilder app) => PrePipeline?.Invoke(app);

    /// <inheritdoc />
    public void OnPreRouting(IApplicationBuilder app) => PreRouting?.Invoke(app);

    /// <inheritdoc />
    public void OnPostRouting(IApplicationBuilder app) => PostRouting?.Invoke(app);

    /// <inheritdoc />
    public void OnPostPipeline(IApplicationBuilder app) => PostPipeline?.Invoke(app);

    /// <inheritdoc />
    public void OnEndpoints(IApplicationBuilder app) => Endpoints?.Invoke(app);
}
