using Dazinator.Extensions.FileProviders.PrependBasePath;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog.Context;
using StackExchange.Profiling;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging.Serilog.Enrichers;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
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
        => new UmbracoApplicationBuilder(app);

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
            app.UseMiddleware<PreviewAuthenticationMiddleware>();
            app.UseMiddleware<UmbracoRequestMiddleware>();
            app.UseMiddleware<MiniProfilerMiddleware>();
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
    ///     Configure a virtual path for backoffice assets to allow cache-busting using the url
    ///     /umbraco/backoffice/!cache-busting-id!/assets/index.js => /umbraco/backoffice/assets/index.js.
    /// </summary>
    /// <remarks>
    ///     The cache busting id is based on a SHA1 hash generated by a number of parameters including the UmbracoVersion and minifier settings.
    ///     A SHA1 has is by definition 40 hexadecimal characters which means we can test for the presence of a 40 character url part and rewrite that.
    /// </remarks>
    /// <see cref="Umbraco.Extensions.UrlHelperExtensions.GetCacheBustHash"/>
    /// <seealso cref="Umbraco.Extensions.StringExtensions.GenerateHash"/>
    /// <seealso cref="https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha1">System.Security.Cryptography.SHA1</seealso>
    public static IApplicationBuilder UseUmbracoBackOfficeRewrites(this IApplicationBuilder builder)
    {
        IHostingEnvironment hostingEnvironment = builder.ApplicationServices.GetRequiredService<IHostingEnvironment>();
        IOptions<GlobalSettings> globalSettings = builder.ApplicationServices.GetRequiredService<IOptions<GlobalSettings>>();
        var backofficePath = globalSettings.Value.GetBackOfficePath(hostingEnvironment).TrimStart("/") ?? "umbraco";

        builder.UseRewriter(new RewriteOptions()
            .AddRewrite(@"^" + backofficePath + @"/backoffice/[a-f0-9]{40}/(.+)", backofficePath + "/backoffice/$1", true));

        return builder;
    }
}
