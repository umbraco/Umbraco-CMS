using System;
using System.Xml;

namespace Umbraco.Core.Embed
{
    public interface IEmbedSettingProvider
    {
       
        object GetSetting(XmlNode settingNode);
    }
}