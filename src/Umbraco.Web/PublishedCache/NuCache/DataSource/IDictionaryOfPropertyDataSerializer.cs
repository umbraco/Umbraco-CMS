using System.Collections.Generic;
using System.IO;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal interface IDictionaryOfPropertyDataSerializer
    {
        IDictionary<string, PropertyData[]> ReadFrom(Stream stream);
        void WriteTo(IDictionary<string, PropertyData[]> value, Stream stream);
    }
}