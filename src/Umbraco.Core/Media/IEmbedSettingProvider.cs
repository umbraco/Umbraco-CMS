using System.Xml;

namespace Umbraco.Core.Media
{
    internal interface IEmbedSettingProvider
    {       
        object GetSetting(XmlNode settingNode);
    }
}