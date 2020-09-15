using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.PublishedCache.NuCache;

namespace Umbraco.PublishedCache.NuCache.LiteDb
{
    public class LiteDbTransactableDictionaryFactory : ITransactableDictionaryFactory
    {
        public ITransactableDictionary<int, ContentNodeKit> Create(string filepath, bool exists)
        {
            var connectionString = filepath;// System.Configuration.ConfigurationManager.ConnectionStrings["LiteDBNuCache"].ConnectionString;
            var name = filepath.Split('\\').Last().Replace(".", "");
            return new LiteDbTransactableDictionary<int, ContentNodeKit>(connectionString, name);
        }
    }
}
