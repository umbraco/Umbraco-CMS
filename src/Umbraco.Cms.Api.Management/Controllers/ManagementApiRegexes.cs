using System.Text.RegularExpressions;

namespace Umbraco.Cms.Api.Management.Controllers;

internal static partial class ManagementApiRegexes
{
    /// <summary>
    /// Gets a regular expression that matches the exact suffix "Controller" at the end of a controller type name.
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches the suffix "Controller" at the end of a string.</returns>
    [GeneratedRegex("(Controller)$")]
    public static partial Regex ControllerTypeToNameRegex();
}
