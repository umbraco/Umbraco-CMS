namespace Umbraco.Cms.Core;

/// <summary>
///     Represents the result of an upgrade check operation.
/// </summary>
public class UpgradeResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UpgradeResult" /> class.
    /// </summary>
    /// <param name="upgradeType">The type of upgrade available.</param>
    /// <param name="comment">A comment or description about the upgrade.</param>
    /// <param name="upgradeUrl">The URL where the upgrade can be obtained.</param>
    public UpgradeResult(string upgradeType, string comment, string upgradeUrl)
    {
        UpgradeType = upgradeType;
        Comment = comment;
        UpgradeUrl = upgradeUrl;
    }

    /// <summary>
    ///     Gets the type of upgrade available (e.g., major, minor, patch).
    /// </summary>
    public string UpgradeType { get; }

    /// <summary>
    ///     Gets a comment or description about the upgrade.
    /// </summary>
    public string Comment { get; }

    /// <summary>
    ///     Gets the URL where the upgrade can be obtained or more information is available.
    /// </summary>
    public string UpgradeUrl { get; }
}
