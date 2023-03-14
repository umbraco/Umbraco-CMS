using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for the PartialViewMacroModel object
/// </summary>
public static class PartialViewMacroModelExtensions
{
    /// <summary>
    ///     Attempt to get a Macro parameter from a PartialViewMacroModel and return a default value otherwise
    /// </summary>
    /// <param name="partialViewMacroModel"></param>
    /// <param name="parameterAlias"></param>
    /// <param name="defaultValue"></param>
    /// <returns>Parameter value if available, the default value that was passed otherwise.</returns>
    public static T? GetParameterValue<T>(this PartialViewMacroModel partialViewMacroModel, string parameterAlias, T defaultValue)
    {
        if (partialViewMacroModel.MacroParameters.ContainsKey(parameterAlias) == false ||
            string.IsNullOrEmpty(partialViewMacroModel.MacroParameters[parameterAlias]?.ToString()))
        {
            return defaultValue;
        }

        Attempt<object?> attempt = partialViewMacroModel.MacroParameters[parameterAlias].TryConvertTo(typeof(T));

        return attempt.Success ? (T?)attempt.Result : defaultValue;
    }

    /// <summary>
    ///     Attempt to get a Macro parameter from a PartialViewMacroModel
    /// </summary>
    /// <param name="partialViewMacroModel"></param>
    /// <param name="parameterAlias"></param>
    /// <returns>Parameter value if available, the default value for the type otherwise.</returns>
    public static T? GetParameterValue<T>(this PartialViewMacroModel partialViewMacroModel, string parameterAlias) =>
        partialViewMacroModel.GetParameterValue(parameterAlias, default(T));
}
