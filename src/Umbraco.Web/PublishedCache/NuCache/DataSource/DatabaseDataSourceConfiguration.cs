using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    public class DatabaseDataSourceConfiguration
    {
        public DatabaseDataSourceConfiguration(bool publishedContentOnly = false)
        {
            PublishedContentOnly = publishedContentOnly;
        }
        public bool PublishedContentOnly { get; private set; }
    }
}
