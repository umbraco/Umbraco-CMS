using System.Linq;
using System.Xml;
using Umbraco.Core.Media;

namespace Umbraco.Web.Media.EmbedProviders.Settings
{
    public class Dictionary : IEmbedSettingProvider
    {
        public object GetSetting(XmlNode settingNode)
        {
            return settingNode.ChildNodes.Cast<XmlNode>().ToDictionary(item => item.Attributes != null ? item.Attributes["name"].Value : null, item => item.InnerText);
        }
    }
}