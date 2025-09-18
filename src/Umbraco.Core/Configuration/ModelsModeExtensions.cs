using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extensions for the <see cref="ModelsMode" /> enumeration.
/// </summary>
[Obsolete("This class will be removed in future versions, as ModelsMode enum will be removed, use modelsmode string instead. Scheduled for removal in V18.")]
public static class ModelsModeExtensions
{
    /// <summary>
    ///     Gets a value indicating whether the mode is *Auto.
    /// </summary>
    public static bool IsAuto(this ModelsMode modelsMode)
        => modelsMode == ModelsMode.InMemoryAuto || modelsMode == ModelsMode.SourceCodeAuto;

    /// <summary>
    ///     Gets a value indicating whether the mode is *Auto but not InMemory.
    /// </summary>
    public static bool IsAutoNotInMemory(this ModelsMode modelsMode)
        => modelsMode == ModelsMode.SourceCodeAuto;

    /// <summary>
    ///     Gets a value indicating whether the mode supports explicit manual generation.
    /// </summary>
    public static bool SupportsExplicitGeneration(this ModelsMode modelsMode)
        => modelsMode == ModelsMode.SourceCodeManual || modelsMode == ModelsMode.SourceCodeAuto;
}
