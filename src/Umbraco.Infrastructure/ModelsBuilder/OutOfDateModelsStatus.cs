using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder;

/// <summary>
///     Used to track if ModelsBuilder models are out of date/stale
/// </summary>
public sealed class OutOfDateModelsStatus : INotificationHandler<ContentTypeCacheRefresherNotification>,
    INotificationHandler<DataTypeCacheRefresherNotification>
{
    private ModelsBuilderSettings _config;
    private readonly IHostEnvironment _hostEnvironment;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OutOfDateModelsStatus" /> class.
    /// </summary>
    public OutOfDateModelsStatus(IOptionsMonitor<ModelsBuilderSettings> config, IHostingEnvironment hostingEnvironment)
    {
        _config = config.CurrentValue;
        _hostEnvironment = StaticServiceProvider.Instance.GetRequiredService<IHostEnvironment>();
        config.OnChange(x => _config = x);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutOfDateModelsStatus"/> class.
    /// </summary>
    /// <param name="config">The options monitor for <see cref="ModelsBuilderSettings"/>.</param>
    /// <param name="hostingEnvironment">The legacy ASP.NET hosting environment.</param>
    /// <param name="hostEnvironment">The generic .NET host environment.</param>
    public OutOfDateModelsStatus(
        IOptionsMonitor<ModelsBuilderSettings> config,
        IHostingEnvironment hostingEnvironment,
        IHostEnvironment hostEnvironment)
    {
        _config = config.CurrentValue;
        _hostEnvironment = hostEnvironment;
        config.OnChange(x => _config = x);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutOfDateModelsStatus"/> class with the specified configuration and host environment.
    /// </summary>
    /// <param name="config">An <see cref="IOptionsMonitor{ModelsBuilderSettings}"/> instance that provides access to ModelsBuilder settings.</param>
    /// <param name="hostEnvironment">An <see cref="IHostEnvironment"/> instance representing the current hosting environment.</param>
    public OutOfDateModelsStatus(IOptionsMonitor<ModelsBuilderSettings> config, IHostEnvironment hostEnvironment)
    {
        _config = config.CurrentValue;
        _hostEnvironment = hostEnvironment;
        config.OnChange(x => _config = x);
    }

    /// <summary>
    ///     Gets a value indicating whether flagging out of date models is enabled
    /// </summary>
    public bool IsEnabled => _config.FlagOutOfDateModels;

    /// <summary>
    ///     Gets a value indicating whether models are out of date
    /// </summary>
    public bool IsOutOfDate
    {
        get
        {
            if (_config.FlagOutOfDateModels == false)
            {
                return false;
            }

            var path = GetFlagPath();
            return path != null && File.Exists(path);
        }
    }

    /// <summary>
    /// Handles the <see cref="ContentTypeCacheRefresherNotification"/> by updating the out-of-date models status.
    /// </summary>
    /// <param name="notification">The notification indicating that the content type cache has been refreshed.</param>
    public void Handle(ContentTypeCacheRefresherNotification notification) => Write();

    /// <summary>
    /// Handles a <see cref="ContentTypeCacheRefresherNotification"/> by updating the out-of-date models status.
    /// </summary>
    /// <param name="notification">The content type cache refresher notification to handle.</param>
    public void Handle(DataTypeCacheRefresherNotification notification) => Write();

    /// <summary>
    /// Clears the out-of-date models flag by deleting the flag file, if it exists. This indicates that the models are no longer considered out-of-date.
    /// </summary>
    public void Clear()
    {
        if (_config.FlagOutOfDateModels == false)
        {
            return;
        }

        var path = GetFlagPath();
        if (!File.Exists(path))
        {
            return;
        }

        File.Delete(path);
    }

    private string GetFlagPath()
    {
        var modelsDirectory = _config.ModelsDirectoryAbsolute(_hostEnvironment);
        if (!Directory.Exists(modelsDirectory))
        {
            Directory.CreateDirectory(modelsDirectory);
        }

        return Path.Combine(modelsDirectory, "ood.flag");
    }

    private void Write()
    {
        // don't run if not configured
        if (!IsEnabled)
        {
            return;
        }

        var path = GetFlagPath();
        if (path == null || File.Exists(path))
        {
            return;
        }

        File.WriteAllText(path, "THIS FILE INDICATES THAT MODELS ARE OUT-OF-DATE\n\n");
    }
}
