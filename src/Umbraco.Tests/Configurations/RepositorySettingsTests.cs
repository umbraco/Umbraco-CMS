using System.Configuration;
using NUnit.Framework;
using Umbraco.Core.Configuration.InfrastructureSettings;

namespace Umbraco.Tests.Configurations
{
    [TestFixture]
    public class RepositorySettingsTests
    {
        [Test]
        public void Can_Get_Repository_From_Config()
        {
            Infrastructure infrastructure = Infrastructure.Instance;
            Repositories repositories = infrastructure.Repositories;
            Repository repository = repositories.Repository["IContentRepository"];

            Assert.That(repository, Is.Not.Null);
            Assert.AreEqual(repository.InterfaceShortTypeName, "IContentRepository");
            Assert.AreEqual(repository.RepositoryFullTypeName, "Umbraco.Core.Persistence.Repositories.ContentRepository, Umbraco.Core");
            Assert.AreEqual(repository.CacheProviderFullTypeName, "Umbraco.Core.Persistence.Caching.RuntimeCacheProvider, Umbraco.Core");
        }

        [Test]
        public void Can_Get_PublishingStrategy_From_Config()
        {
            Infrastructure infrastructure = Infrastructure.Instance;
            PublishingProvider strategy = infrastructure.PublishingStrategy;

            Assert.That(strategy.Type, Is.EqualTo("Umbraco.Web.Publishing.PublishingStrategy, Umbraco.Web"));
        }
    }
}