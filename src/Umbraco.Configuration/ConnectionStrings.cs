using System;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration
{
    public class ConnectionStrings : IConnectionStrings
    {
        private readonly IIOHelper _ioHelper;

        public ConnectionStrings(IIOHelper ioHelper)
        {
            _ioHelper = ioHelper;
        }

        public ConfigConnectionString this[string key]
        {
            get
            {
                var settings = ConfigurationManager.ConnectionStrings[key];
                if (settings == null) return null;
                return new ConfigConnectionString(settings.ConnectionString, settings.ProviderName, settings.Name);
            }
        }

        public void RemoveConnectionString(string key)
        {
            var fileName = _ioHelper.MapPath(string.Format("{0}/web.config", _ioHelper.Root));
            var xml = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);

            var appSettings = xml.Root.DescendantsAndSelf("appSettings").Single();
            var setting = appSettings.Descendants("add").FirstOrDefault(s => s.Attribute("key").Value == key);

            if (setting != null)
            {
                setting.Remove();
                xml.Save(fileName, SaveOptions.DisableFormatting);
                ConfigurationManager.RefreshSection("appSettings");
            }
            var settings = ConfigurationManager.ConnectionStrings[key];
        }
    }
}
