using System.Xml;

namespace Umbraco.Core.Media
{
    public interface IEmbedSettingProvider
    {       
        object GetSetting(XmlNode settingNode);
    }
}