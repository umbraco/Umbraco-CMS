// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for models builder settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigModelsBuilder, BindNonPublicProperties = true)]
public class ModelsBuilderSettings
{
    internal const string StaticModelsMode = "InMemoryAuto";
    internal const string StaticModelsDirectory = "~/umbraco/models";
    internal const bool StaticAcceptUnsafeModelsDirectory = false;
    internal const int StaticDebugLevel = 0;
    private bool _flagOutOfDateModels = true;

    /// <summary>
    ///     Gets or sets a value for the models mode.
    /// </summary>
    [DefaultValue(StaticModelsMode)]
    public ModelsMode ModelsMode { get; set; } = Enum<ModelsMode>.Parse(StaticModelsMode);

    /// <summary>
    ///     Gets or sets a value for models namespace.
    /// </summary>
    /// <remarks>That value could be overriden by other (attribute in user's code...). Return default if no value was supplied.</remarks>
    [DefaultValue(Constants.ModelsBuilder.DefaultModelsNamespace)]
    public string ModelsNamespace { get; set; } = Constants.ModelsBuilder.DefaultModelsNamespace;

    /// <summary>
    ///     Gets or sets a value indicating whether we should flag out-of-date models.
    /// </summary>
    /// <remarks>
    ///     Models become out-of-date when data types or content types are updated. When this
    ///     setting is activated the ~/umbraco/models/PureLive/ood.txt file is then created. When models are
    ///     generated through the dashboard, the files is cleared. Default value is <c>false</c>.
    /// </remarks>
    public bool FlagOutOfDateModels
    {
        get
        {
            if (ModelsMode == ModelsMode.Nothing ||ModelsMode.IsAuto())
            {
                return false;

            }

            return _flagOutOfDateModels;
        }

            set => _flagOutOfDateModels = value;
    }


    /// <summary>
    ///     Gets or sets a value for the models directory.
    /// </summary>
    /// <remarks>Default is ~/umbraco/models but that can be changed.</remarks>
    [DefaultValue(StaticModelsDirectory)]
    public string ModelsDirectory { get; set; } = StaticModelsDirectory;

    /// <summary>
    ///     Gets or sets a value indicating whether to accept an unsafe value for ModelsDirectory.
    /// </summary>
    /// <remarks>
    ///     An unsafe value is an absolute path, or a relative path pointing outside
    ///     of the website root.
    /// </remarks>
    [DefaultValue(StaticAcceptUnsafeModelsDirectory)]
    public bool AcceptUnsafeModelsDirectory { get; set; } = StaticAcceptUnsafeModelsDirectory;

    /// <summary>
    ///     Gets or sets a value indicating the debug log level.
    /// </summary>
    /// <remarks>0 means minimal (safe on live site), anything else means more and more details (maybe not safe).</remarks>
    [DefaultValue(StaticDebugLevel)]
    public int DebugLevel { get; set; } = StaticDebugLevel;
}
