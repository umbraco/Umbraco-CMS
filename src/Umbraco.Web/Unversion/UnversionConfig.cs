using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Unversion
{
    public class UnversionConfig : IUnversionConfig
    {
        public const string AllDocumentTypesKey = "$_ALL";
        public IDictionary<string, List<UnversionConfigEntry>> ConfigEntries { get; set; }

        private ILogger _logger;

        public UnversionConfig(ILogger logger)
        {
            _logger = logger;

            ConfigEntries = new Dictionary<string, List<UnversionConfigEntry>>();

            try
            {
                var appPath = HttpRuntime.AppDomainAppPath;
                var configFilePath = Path.Combine(appPath, @"config\unversion.config");
                LoadXmlConfig(string.Concat(configFilePath));
            }
            catch (Exception e)
            {
                _logger.Error<UnversionConfig>(e, "Error when parsing unversion.config.");
            }

        }

        private void LoadXmlConfig(string configPath)
        {
            if (!File.Exists(configPath))
            {
                _logger.Warn<UnversionConfig>("Couldn't find config file {configPath}", configPath);
                return;
            }

            var xmlConfig = new XmlDocument();
            xmlConfig.Load(configPath);

            foreach (XmlNode xmlConfigEntry in xmlConfig.SelectNodes("/unversionConfig/add"))
                if (xmlConfigEntry.NodeType == XmlNodeType.Element)
                {
                    var configEntry = new UnversionConfigEntry
                    {
                        DocTypeAlias = xmlConfigEntry.Attributes["docTypeAlias"] != null
                            ? xmlConfigEntry.Attributes["docTypeAlias"].Value
                            : AllDocumentTypesKey
                    };

                    if (xmlConfigEntry.Attributes["rootXpath"] != null)
                        configEntry.RootXPath = xmlConfigEntry.Attributes["rootXpath"].Value;

                    if (xmlConfigEntry.Attributes["maxDays"] != null)
                        configEntry.MaxDays = Convert.ToInt32(xmlConfigEntry.Attributes["maxDays"].Value);

                    if (xmlConfigEntry.Attributes["maxCount"] != null)
                        configEntry.MaxCount = Convert.ToInt32(xmlConfigEntry.Attributes["maxCount"].Value);

                    if (xmlConfigEntry.Attributes["minCount"] != null)
                        configEntry.MinCount = Convert.ToInt32(xmlConfigEntry.Attributes["minCount"].Value);

                    if (!ConfigEntries.ContainsKey(configEntry.DocTypeAlias))
                        ConfigEntries.Add(configEntry.DocTypeAlias, new List<UnversionConfigEntry>());

                    ConfigEntries[configEntry.DocTypeAlias].Add(configEntry);
                }
        }
    }

    public class UnversionConfigEntry
    {
        public UnversionConfigEntry()
        {
            MaxDays = MaxCount = MinCount = int.MaxValue;
        }

        public string DocTypeAlias { get; set; }
        public string RootXPath { get; set; }
        public int MaxDays { get; set; }
        public int MaxCount { get; set; }
        public int MinCount { get; set; }
    }

    public interface IUnversionConfig
    {
        IDictionary<string, List<UnversionConfigEntry>> ConfigEntries { get; }
    }
}
