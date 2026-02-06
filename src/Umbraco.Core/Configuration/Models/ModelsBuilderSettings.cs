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
    /// <summary>
    ///     The default models mode.
    /// </summary>
    internal const string StaticModelsMode = "InMemoryAuto";

    /// <summary>
    ///     The default models directory path.
    /// </summary>
    internal const string StaticModelsDirectory = "~/umbraco/models";

    /// <summary>
    ///     The default value for accepting unsafe models directory.
    /// </summary>
    internal const bool StaticAcceptUnsafeModelsDirectory = false;

    /// <summary>
    ///     The default debug level.
    /// </summary>
    internal const int StaticDebugLevel = 0;

    /// <summary>
    ///     The default value for including version number in generated models.
    /// </summary>
    internal const bool StaticIncludeVersionNumberInGeneratedModels = true;

    /// <summary>
    ///     The default value for generating virtual properties.
    /// </summary>
    internal const bool StaticGenerateVirtualProperties = true;

    private bool _flagOutOfDateModels = true;

    /// <summary>
    ///     Gets or sets a value for the models mode.
    /// </summary>
    [DefaultValue(StaticModelsMode)]
    public string ModelsMode { get; set; } = StaticModelsMode;

    /// <summary>
    ///     Gets or sets a value for models namespace.
    /// </summary>
    /// <remarks>That value could be overriden by other (attribute in user's code...). Return default if no value was supplied.</remarks>
    [DefaultValue(Constants.ModelsBuilder.DefaultModelsNamespace)]
    public string ModelsNamespace { get; set; } = Constants.ModelsBuilder.DefaultModelsNamespace;

    /// <summary>
    ///     Gets or sets a value indicating whether we should flag out-of-date models.
    /// </summary>
    public bool FlagOutOfDateModels
    {
        get => ModelsMode == Constants.ModelsBuilder.ModelsModes.SourceCodeManual && _flagOutOfDateModels;

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

    /// <summary>
    ///     Gets or sets a value indicating whether the version number should be included in generated models.
    /// </summary>
    /// <remarks>
    ///     By default this is written to the <see cref="System.CodeDom.Compiler.GeneratedCodeAttribute"/> output in
    ///     generated code for each property of the model. This can be useful for debugging purposes but isn't essential,
    ///     and it has the causes the generated code to change every time Umbraco is upgraded. In turn, this leads
    ///     to unnecessary code file changes that need to be checked into source control. Default is <c>true</c>.
    /// </remarks>
    [DefaultValue(StaticIncludeVersionNumberInGeneratedModels)]
    public bool IncludeVersionNumberInGeneratedModels { get; set; } = StaticIncludeVersionNumberInGeneratedModels;

    /// <summary>
    ///     Gets or sets a value indicating whether to mark all properties in the generated models as virtual.
    /// </summary>
    /// <remarks>
    ///     Virtual properties will not work with Hot Reload when running dotnet watch.
    /// </remarks>
    [DefaultValue(StaticGenerateVirtualProperties)]
    public bool GenerateVirtualProperties { get; set; } = StaticGenerateVirtualProperties;
}
