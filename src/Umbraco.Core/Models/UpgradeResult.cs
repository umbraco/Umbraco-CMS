namespace Umbraco.Core.Models
{
    public class UpgradeResult
    {
        public string UpgradeType { get; }
        public string Comment { get; }
        public string UpgradeUrl { get; }

        public UpgradeResult(string upgradeType, string comment, string upgradeUrl)
        {
            UpgradeType = upgradeType;
            Comment = comment;
            UpgradeUrl = upgradeUrl;
        }
    }
}
