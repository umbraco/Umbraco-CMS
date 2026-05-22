using Dazinator.Extensions.FileProviders.PrependBasePath;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Serilog.Context;
using StackExchange.Profiling;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging.Serilog.Enrichers;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Cms.Web.Common.HealthChecks;
using Umbraco.Cms.Web.Common.Hosting;
using Umbraco.Cms.Web.Common.Media;
using Umbraco.Cms.Web.Common.Middleware;
using Umbraco.Cms.Web.Common.Plugins;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Extensions;

/// <summary>
///     <see cref="IApplicationBuilder" /> extensions for Umbraco
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    ///     Configures and use services required for using Umbraco
    /// </summary>
    public static IUmbracoApplicationBuilder UseUmbraco(this IApplicationBuilder app)
    {
        // Ensure Umbraco is booted and StaticServiceProvider.Instance is set before continuing
        IRuntimeState runtimeState = app.ApplicationServices.GetRequiredService<IRuntimeState>();
        if (runtimeState.Level == RuntimeLevel.Unknown)
        {
            throw new BootFailedException("The runtime level is unknown, please make sure Umbraco is booted by adding `await app.BootUmbracoAsync();` just after `WebApplication app = builder.Build();` in your Program.cs file.");
        }

        if (StaticServiceProvider.Instance is null)
        {
            throw new BootFailedException("StaticServiceProvider.Instance is not set, please make sure ConfigureUmbracoDefaults() is added in your Program.cs file.");
        }

        return new UmbracoApplicationBuilder(app);
    }

    /// <summary>
    ///     Returns true if Umbraco <see cref="IRuntimeState" /> is greater than <see cref="RuntimeLevel.BootFailed" />
    /// </summary>
    public static bool UmbracoCanBoot(this IApplicationBuilder app)
        => app.ApplicationServices.GetRequiredService<IRuntimeState>().UmbracoCanBoot();

    /// <summary>
    ///     Enables core Umbraco functionality
    /// </summary>
    public static IApplicationBuilder UseUmbracoCore(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (!app.UmbracoCanBoot())
        {
            return app;
        }

        // Register our global threadabort enricher for logging
        ThreadAbortExceptionEnricher threadAbortEnricher =
            app.ApplicationServices.GetRequiredService<ThreadAbortExceptionEnricher>();
        LogContext.Push(
            threadAbortEnricher); // NOTE: We are not in a using clause because we are not removing it, it is on the global context

        return app;
    }

    /// <summary>
    ///     Enables middlewares required to run Umbraco
    /// </summary>
    /// <remarks>
    ///     Must occur before UseRouting
    /// </remarks>
    public static IApplicationBuilder UseUmbracoRouting(this IApplicationBuilder app)
    {
        // TODO: This method could be internal or part of another call - this is a required system so should't be 'opt-in'
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (!app.UmbracoCanBoot())
        {
            app.UseStaticFiles(); // We need static files to show the nice error page.
            app.UseMiddleware<BootFailedMiddleware>();
        }
        else
        {
            // BootFailedMiddleware must also be registered on the boot-success path because
            // RuntimeLevel.BootFailed can be set at runtime by UnattendedUpgradeBackgroundService
            // if a background migration fails after the HTTP server has already started.
            app.UseMiddleware<BootFailedMiddleware>();

            // Health probes are registered before other middleware so they are reachable
            // during Upgrading state. They are intercepted by BootFailedMiddleware when
            // the runtime transitions to BootFailed after an upgrade failure.
            app.UseUmbracoHealthChecks();

            app.UseMiddleware<PreviewAuthenticationMiddleware>();
            app.UseMiddleware<UmbracoRequestMiddleware>();
            app.UseMiddleware<MiniProfilerMiddleware>();
            app.UseMiddleware<ProtectRecycleBinMediaMiddleware>();
        }

        return app;
    }

    /// <summary>
    ///     Adds request based serilog enrichers to the LogContext for each request
    /// </summary>
    public static IApplicationBuilder UseUmbracoRequestLogging(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (!app.UmbracoCanBoot())
        {
            return app;
        }

        app.UseMiddleware<UmbracoRequestLoggingMiddleware>();

        return app;
    }

    /// <summary>
    ///     Allow static file access for App_Plugins folders
    /// </summary>
    public static IApplicationBuilder UseUmbracoPluginsStaticFiles(this IApplicationBuilder app)
    {
        IHostEnvironment hostingEnvironment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

        var pluginFolder = hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.AppPlugins);
        if (Directory.Exists(pluginFolder))
        {
            IOptionsMonitor<UmbracoPluginSettings> umbracoPluginSettings =
                app.ApplicationServices.GetRequiredService<IOptionsMonitor<UmbracoPluginSettings>>();

            var pluginFileProvider = new UmbracoPluginPhysicalFileProvider(
                pluginFolder,
                umbracoPluginSettings);

            IWebHostEnvironment? webHostEnvironment = app.ApplicationServices.GetService<IWebHostEnvironment>();

            if (webHostEnvironment is not null)
            {
                webHostEnvironment.WebRootFileProvider = webHostEnvironment.WebRootFileProvider.ConcatComposite(
                    new PrependBasePathFileProvider(Constants.SystemDirectories.AppPlugins, pluginFileProvider));
            }
        }

        return app;
    }

    /// <summary>
    ///     Registers Umbraco health probe endpoints.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <c>GET /umbraco/api/health/live</c> — always 200 while the process is alive.
    ///     </para>
    ///     <para>
    ///         <c>GET /umbraco/api/health/ready</c> — 200 only when <see cref="RuntimeLevel.Run"/>,
    ///         503 Degraded during <see cref="RuntimeLevel.Upgrading"/> and other non-Run states.
    ///     </para>
    /// </remarks>
    public static IApplicationBuilder UseUmbracoHealthChecks(this IApplicationBuilder app)
    {
        // Liveness — always 200 if the process responds (no custom checks).
        app.UseHealthChecks("/umbraco/api/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            AllowCachingResponses = false,
        });

        // Readiness — 200 only when RuntimeLevel.Run.
        app.UseHealthChecks("/umbraco/api/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(UmbracoReadinessHealthCheck.ReadyTag),
            AllowCachingResponses = false,
        });

        return app;
    }

    /// <summary>
    ///     Configure custom umbraco file provider for media
    /// </summary>
    public static IApplicationBuilder UseUmbracoMediaFileProvider(this IApplicationBuilder app)
    {
        // Get media file provider and request path/URL
        MediaFileManager mediaFileManager = app.ApplicationServices.GetRequiredService<MediaFileManager>();
        if (mediaFileManager.FileSystem.TryCreateFileProvider(out IFileProvider? mediaFileProvider))
        {
            GlobalSettings globalSettings =
                app.ApplicationServices.GetRequiredService<IOptions<GlobalSettings>>().Value;
            IHostingEnvironment? hostingEnvironment = app.ApplicationServices.GetService<IHostingEnvironment>();
            var mediaRequestPath = hostingEnvironment?.ToAbsolute(globalSettings.UmbracoMediaPath);

            // Configure custom file provider for media
            IWebHostEnvironment? webHostEnvironment = app.ApplicationServices.GetService<IWebHostEnvironment>();
            if (webHostEnvironment is not null)
            {
                webHostEnvironment.WebRootFileProvider =
                    webHostEnvironment.WebRootFileProvider.ConcatComposite(
                        new MediaPrependBasePathFileProvider(mediaRequestPath, mediaFileProvider));
            }
        }

        return app;
    }

    /// <summary>
    ///     Sets the default <c>Cache-Control</c> response header on requests served from the cache-busted
    ///     BackOffice assets path (<c>/umbraco/backoffice/&lt;hash&gt;/...</c>).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The path prefix contains a deployment-wide hash derived from the Umbraco version
    ///         (see <see cref="IBackOfficePathGenerator.BackOfficeCacheBustHash"/>). Because the URL itself
    ///         changes whenever the version changes, all responses served under that prefix are safe to mark
    ///         as <c>immutable</c> with a long <c>max-age</c>, regardless of whether the on-disk filename
    ///         contains a content hash.
    ///     </para>
    ///     <para>
    ///         In debug mode the cache-busting hash changes on every request, so the header is set to
    ///         <c>no-cache</c> to avoid filling the browser disk cache with single-use entries.
    ///     </para>
    ///     <para>
    ///         This middleware is non-destructive to consumer customisation:
    ///         <list type="bullet">
    ///             <item>
    ///                 The header is only set when no <c>Cache-Control</c> value is already present on the
    ///                 response, so synchronous overrides written upstream (including
    ///                 <c>StaticFileOptions.OnPrepareResponse</c>) take precedence.
    ///             </item>
    ///             <item>
    ///                 The header is set via <c>HttpResponse.OnStarting</c>; consumer callbacks registered
    ///                 later in the pipeline fire first (LIFO) and can therefore override the default.
    ///             </item>
    ///             <item>
    ///                 Non-2xx responses (e.g. 404) are not marked as immutable to avoid long-lived caching
    ///                 of error responses.
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Must be registered before <see cref="UseUmbracoBackOfficeRewrites"/> so that the original
    ///         request path (still containing the cache-bust hash) can be matched.
    ///     </para>
    /// </remarks>
    public static IApplicationBuilder UseUmbracoBackOfficeCacheHeaders(this IApplicationBuilder builder)
    {
        IBackOfficePathGenerator backOfficePathGenerator = builder.ApplicationServices.GetRequiredService<IBackOfficePathGenerator>();
        IHostingEnvironment hostingEnvironment = builder.ApplicationServices.GetRequiredService<IHostingEnvironment>();

        // BackOfficeAssetsPath looks like "/umbraco/backoffice/<hash>" (no trailing slash).
        var prefix = "/" + backOfficePathGenerator.BackOfficeAssetsPath.TrimStart('/').TrimEnd('/');
        var headerValue = hostingEnvironment.IsDebugMode
            ? "no-cache"
            : "public, max-age=31536000, immutable";

        return builder.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.OnStarting(static state =>
                {
                    (HttpResponse response, string value) = ((HttpResponse, string))state;
                    if (response.StatusCode is >= 200 and < 300
                        && !response.Headers.ContainsKey(HeaderNames.CacheControl))
                    {
                        response.Headers[HeaderNames.CacheControl] = value;
                    }

                    return Task.CompletedTask;
                }, (context.Response, headerValue));
            }

            await next();
        });
    }

    /// <summary>
    ///     Configure a virtual path with IApplicationBuilder.UseRewriter for BackOffice assets to allow cache-busting using the url
    ///     /umbraco/backoffice/!cache-busting-id!/assets/index.js => /umbraco/backoffice/assets/index.js.
    /// </summary>
    public static IApplicationBuilder UseUmbracoBackOfficeRewrites(this IApplicationBuilder builder)
    {
        IBackOfficePathGenerator backOfficePathGenerator = builder.ApplicationServices.GetRequiredService<IBackOfficePathGenerator>();
        var backOfficeAssetsPath = backOfficePathGenerator.BackOfficeAssetsPath.TrimStart("/").EnsureEndsWith("/");

        builder.UseRewriter(new RewriteOptions()
            // The destination needs to be hardcoded to "/umbraco/backoffice" because this is where they are located in the Umbraco.Cms.StaticAssets RCL
            .AddRewrite(@"^" + backOfficeAssetsPath + "(.+)",  "/umbraco/backoffice/$1", true));

        return builder;
    }
}
