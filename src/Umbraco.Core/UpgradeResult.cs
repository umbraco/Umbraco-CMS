namespace Umbraco.Cms.Core;

public class UpgradeResult
{
    public UpgradeResult(string upgradeType, string comment, string upgradeUrl)
    {
        UpgradeType = upgradeType;
        Comment = comment;
        UpgradeUrl = upgradeUrl;
    }

    public string UpgradeType { get; }

    public string Comment { get; }

    public string UpgradeUrl { get; }
}
