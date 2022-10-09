using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder;

/// <summary>
///     Used to track if ModelsBuilder models are out of date/stale
/// </summary>
public sealed class OutOfDateModelsStatus : INotificationHandler<ContentTypeCacheRefresherNotification>,
    INotificationHandler<DataTypeCacheRefresherNotification>
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private ModelsBuilderSettings _config;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OutOfDateModelsStatus" /> class.
    /// </summary>
    public OutOfDateModelsStatus(IOptionsMonitor<ModelsBuilderSettings> config, IHostingEnvironment hostingEnvironment)
    {
        _config = config.CurrentValue;
        _hostingEnvironment = hostingEnvironment;
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

    public void Handle(ContentTypeCacheRefresherNotification notification) => Write();

    public void Handle(DataTypeCacheRefresherNotification notification) => Write();

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
        var modelsDirectory = _config.ModelsDirectoryAbsolute(_hostingEnvironment);
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
