using System.Xml;
using Umbraco.Core.Media;

namespace Umbraco.Web.Media.EmbedProviders.Settings
{
    public class String : IEmbedSettingProvider
    {
        public object GetSetting(XmlNode settingNode)
        {
            return settingNode.InnerText;
        }
    }
}
