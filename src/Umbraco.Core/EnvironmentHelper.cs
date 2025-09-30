using Umbraco.Extensions;

namespace Umbraco.Cms.Core;

/// <summary>
///     Currently just used to get the machine name for use with file names
/// </summary>
internal static class EnvironmentHelper
{
    /// <summary>
    ///     Returns the machine name that is safe to use in file paths.
    /// </summary>
    public static string FileSafeMachineName => Environment.MachineName.ReplaceNonAlphanumericChars('-');
}
