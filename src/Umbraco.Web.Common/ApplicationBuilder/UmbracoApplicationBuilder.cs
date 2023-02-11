using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Media;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

/// <summary>
///     A builder used to enable middleware and endpoints required for Umbraco to operate.
/// </summary>
/// <remarks>
///     This helps to ensure that everything is registered in the correct order.
/// </remarks>
public class UmbracoApplicationBuilder : IUmbracoApplicationBuilder, IUmbracoEndpointBuilder,
    IUmbracoApplicationBuilderContext
{
    private readonly IOptions<UmbracoPipelineOptions> _umbracoPipelineStartupOptions;

    public UmbracoApplicationBuilder(IApplicationBuilder appBuilder)
    {
        AppBuilder = appBuilder ?? throw new ArgumentNullException(nameof(appBuilder));
        ApplicationServices = appBuilder.ApplicationServices;
        RuntimeState = appBuilder.ApplicationServices.GetRequiredService<IRuntimeState>();
        _umbracoPipelineStartupOptions = ApplicationServices.GetRequiredService<IOptions<UmbracoPipelineOptions>>();
    }

    public IServiceProvider ApplicationServices { get; }

    public IRuntimeState RuntimeState { get; }

    public IApplicationBuilder AppBuilder { get; }

    /// <inheritdoc />
    public IUmbracoEndpointBuilder WithCustomMiddleware(
        Action<IUmbracoApplicationBuilderContext> configureUmbracoMiddleware)
    {
        if (configureUmbracoMiddleware is null)
        {
            throw new ArgumentNullException(nameof(configureUmbracoMiddleware));
        }

        configureUmbracoMiddleware(this);

        return this;
    }

    /// <inheritdoc />
    public IUmbracoEndpointBuilder WithMiddleware(Action<IUmbracoApplicationBuilderContext> configureUmbracoMiddleware)
    {
        if (configureUmbracoMiddleware is null)
        {
            throw new ArgumentNullException(nameof(configureUmbracoMiddleware));
        }

        RunPrePipeline();

        RegisterDefaultRequiredMiddleware();

        RunPostPipeline();

        configureUmbracoMiddleware(this);

        return this;
    }

    /// <summary>
    ///     Registers the default required middleware to run Umbraco.
    /// </summary>
    public void RegisterDefaultRequiredMiddleware()
    {
        UseUmbracoCoreMiddleware();

        // Important we handle image manipulations before the static files, otherwise the querystring is just ignored.
        AppBuilder.UseImageSharp();

        // Get media file provider and request path/URL
        MediaFileManager mediaFileManager = AppBuilder.ApplicationServices.GetRequiredService<MediaFileManager>();
        if (mediaFileManager.FileSystem.TryCreateFileProvider(out IFileProvider? mediaFileProvider))
        {
            GlobalSettings globalSettings =
                AppBuilder.ApplicationServices.GetRequiredService<IOptions<GlobalSettings>>().Value;
            IHostingEnvironment? hostingEnvironment = AppBuilder.ApplicationServices.GetService<IHostingEnvironment>();
            var mediaRequestPath = hostingEnvironment?.ToAbsolute(globalSettings.UmbracoMediaPath);

            // Configure custom file provider for media
            IWebHostEnvironment? webHostEnvironment = AppBuilder.ApplicationServices.GetService<IWebHostEnvironment>();
            if (webHostEnvironment is not null)
            {
                webHostEnvironment.WebRootFileProvider =
                    webHostEnvironment.WebRootFileProvider.ConcatComposite(
                        new MediaPrependBasePathFileProvider(mediaRequestPath, mediaFileProvider));
            }
        }

        AppBuilder.UseStaticFiles();

        AppBuilder.UseUmbracoPluginsStaticFiles();

        // UseRouting adds endpoint routing middleware, this means that middlewares registered after this one
        // will execute after endpoint routing. The ordering of everything is quite important here, see
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-5.0
        // where we need to have UseAuthentication and UseAuthorization proceeding this call but before
        // endpoints are defined.
        AppBuilder.UseRouting();
        AppBuilder.UseAuthentication();
        AppBuilder.UseAuthorization();

        // This must come after auth because the culture is based on the auth'd user
        AppBuilder.UseRequestLocalization();

        // Must be called after UseRouting and before UseEndpoints
        AppBuilder.UseSession();

        // DO NOT PUT ANY UseEndpoints declarations here!! Those must all come very last in the pipeline,
        // endpoints are terminating middleware. All of our endpoints are declared in ext of IUmbracoApplicationBuilder
    }

    public void UseUmbracoCoreMiddleware()
    {
        AppBuilder.UseUmbracoCore();
        AppBuilder.UseUmbracoRequestLogging();

        // We need to add this before UseRouting so that the UmbracoContext and other middlewares are executed
        // before endpoint routing middleware.
        AppBuilder.UseUmbracoRouting();
    }

    public void RunPrePipeline()
    {
        foreach (IUmbracoPipelineFilter filter in _umbracoPipelineStartupOptions.Value.PipelineFilters)
        {
            filter.OnPrePipeline(AppBuilder);
        }
    }

    public void RunPostPipeline()
    {
        foreach (IUmbracoPipelineFilter filter in _umbracoPipelineStartupOptions.Value.PipelineFilters)
        {
            filter.OnPostPipeline(AppBuilder);
        }
    }

    /// <inheritdoc />
    public void WithEndpoints(Action<IUmbracoEndpointBuilderContext> configureUmbraco)
    {
        RunPreEndpointsPipeline();

        AppBuilder.UseEndpoints(endpoints =>
        {
            var umbAppBuilder =
                (IUmbracoEndpointBuilderContext)ActivatorUtilities.CreateInstance<UmbracoEndpointBuilder>(
                    ApplicationServices, AppBuilder, endpoints);
            configureUmbraco(umbAppBuilder);
        });
    }

    private void RunPreEndpointsPipeline()
    {
        foreach (IUmbracoPipelineFilter filter in _umbracoPipelineStartupOptions.Value.PipelineFilters)
        {
            filter.OnEndpoints(AppBuilder);
        }
    }
}
