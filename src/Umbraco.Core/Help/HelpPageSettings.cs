using System.Configuration;

namespace Umbraco.Core.Help
{
    public class HelpPageSettings : IHelpPageSettings
    {
        public string HelpPageUrlAllowList =>
            ConfigurationManager.AppSettings.ContainsKey(Constants.AppSettings.HelpPageUrlAllowList)
                ? ConfigurationManager.AppSettings[Constants.AppSettings.HelpPageUrlAllowList]
                : null;
    }
}
