using System.IO;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Infrastructure.Persistence
{
    public class SQLiteEmbeddedDatabaseCreator : IEmbeddedDatabaseCreator
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public SQLiteEmbeddedDatabaseCreator(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public string ProviderName => Cms.Core.Constants.DatabaseProviders.SQLite;

        public string ConnectionString { get; set; }

        public void Create()
        {
            // NOTE: Not sure we need to do anything as SQLite will create file if it does not exist when opening connection

            // Create file on disk from standard path....
            //var path = _hostingEnvironment.MapPathContentRoot(Path.Combine(Constants.SystemDirectories.Data, "Umbraco.db"));
            //if (File.Exists(path) == false)
            //{
            //    File.Create(path);
            //}
        }
    }
}
