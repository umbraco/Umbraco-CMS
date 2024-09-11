using Examine;
using Examine.Lucene.Directories;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Infrastructure.Examine
{
    /// <summary>
    /// Custom version of https://github.com/Shazwazza/Examine/blob/release/3.0/src/Examine.Lucene/Directories/TempEnvFileSystemDirectoryFactory.cs that includes the Umbraco SiteName property in the path hash
    /// </summary>
    public class UmbracoTempEnvFileSystemDirectoryFactory : FileSystemDirectoryFactory
    {
        public UmbracoTempEnvFileSystemDirectoryFactory(
            IApplicationIdentifier applicationIdentifier,
            ILockFactory lockFactory,
            IHostingEnvironment hostingEnvironment)
            : base(new DirectoryInfo(GetTempPath(applicationIdentifier, hostingEnvironment)), lockFactory)
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
