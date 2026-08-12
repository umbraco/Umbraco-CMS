using Examine;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.DependencyInjection;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Infrastructure.Examine
{
    /// <summary>
    /// Custom version of https://github.com/Shazwazza/Examine/blob/release/3.0/src/Examine.Lucene/Directories/TempEnvFileSystemDirectoryFactory.cs that includes the Umbraco SiteName property in the path hash
    /// </summary>
    public class UmbracoTempEnvFileSystemDirectoryFactory : FileSystemDirectoryFactory
    {
        [Obsolete("Use the constructor accepting IOptionsMonitor<LuceneDirectoryIndexOptions>. Scheduled for removal in Umbraco 20.")]
        public UmbracoTempEnvFileSystemDirectoryFactory(
            IApplicationIdentifier applicationIdentifier,
            ILockFactory lockFactory,
            IHostingEnvironment hostingEnvironment)
            : this(
                applicationIdentifier,
                lockFactory,
                hostingEnvironment,
                StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<LuceneDirectoryIndexOptions>>())
        {
        }

        public UmbracoTempEnvFileSystemDirectoryFactory(
            IApplicationIdentifier applicationIdentifier,
            ILockFactory lockFactory,
            IHostingEnvironment hostingEnvironment,
            IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions)
            : base(new DirectoryInfo(GetTempPath(applicationIdentifier, hostingEnvironment)), lockFactory, indexOptions)
        {
        }

        public static string GetTempPath(IApplicationIdentifier applicationIdentifier, IHostingEnvironment hostingEnvironment)
        {
            var hashString = hostingEnvironment.SiteName + "::" + applicationIdentifier.GetApplicationUniqueIdentifier();
            var appDomainHash = hashString.GenerateHash();

            var cachePath = Path.Combine(
                Path.GetTempPath(),
                "ExamineIndexes",
                //include the appdomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
                // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
                // utilizing an old index
                appDomainHash);

            return cachePath;
        }
    }
}
