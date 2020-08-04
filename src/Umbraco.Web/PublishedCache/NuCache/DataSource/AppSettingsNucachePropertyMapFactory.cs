using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    public class AppSettingsNuCachePropertyMapFactory : INuCachePropertyOptionsFactory
    {
        public NucachePropertyOptions GetNuCachePropertyOptions()
        {
            NucachePropertyOptions options = new NucachePropertyOptions
            {
                PropertyMap = GetPropertyMap(),
                LZ4CompressionLevel = K4os.Compression.LZ4.LZ4Level.L10_OPT,
                MinimumCompressibleStringLength = null
            };
            return options;
        }

        public IReadOnlyDictionary<string, (NucachePropertyCompressionLevel compress, NucachePropertyDecompressionLevel decompressionLevel, string mappedAlias)> GetPropertyMap()
        {
            var propertyMap = new Dictionary<string, (NucachePropertyCompressionLevel compress, NucachePropertyDecompressionLevel decompressionLevel, string mappedAlias)>();
            // TODO: Use xml/json/c# to define map
            var propertyDictionarySerializerMap = ConfigurationManager.AppSettings["Umbraco.Web.PublishedCache.NuCache.PropertySerializationMap"];
            if (!string.IsNullOrWhiteSpace(propertyDictionarySerializerMap))
            {
                //propertyAlias,CompressionLevel,DecompressionLevel,mappedAlias;
                propertyDictionarySerializerMap.Split(';')
                    .Select(x =>
                    {
                        var y = x.Split(',');
                        (string alias, NucachePropertyCompressionLevel compressionLevel, NucachePropertyDecompressionLevel decompressionLevel, string mappedAlias) v = (y[0],
                        (NucachePropertyCompressionLevel)System.Enum.Parse(typeof(NucachePropertyCompressionLevel), y[1]),
                        (NucachePropertyDecompressionLevel)System.Enum.Parse(typeof(NucachePropertyDecompressionLevel), y[2]),
                        y[3]
                        );
                        return v;
                    })
                    .ToList().ForEach(x => propertyMap.Add(x.alias, (x.compressionLevel, x.decompressionLevel, x.mappedAlias)));
            }
            return propertyMap;
        }
    }
}
