using System.Text.RegularExpressions;

namespace Umbraco.Cms.Api.Management.Controllers;

internal static partial class ManagementApiRegexes
{
    [GeneratedRegex("(Controller)$")]
    public static partial Regex ControllerTypeToNameRegex();
}
