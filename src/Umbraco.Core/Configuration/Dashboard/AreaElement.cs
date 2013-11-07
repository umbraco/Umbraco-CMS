using System.Configuration;

namespace Umbraco.Core.Configuration.Dashboard
{
    internal class AreaElement : InnerTextConfigurationElement<string>, IArea
    {
        string IArea.AreaName
        {
            get { return Value; }
        }
    }
}