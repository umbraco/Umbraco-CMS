using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class RedirectUrlServiceTests : UmbracoIntegrationTestWithContent
    {
        private IContent _testPage;
        private IContent _altTestPage;
        private const string Url = "blah";
        private const string CultureA = "en";
        private const string CultureB = "de";

        private IRedirectUrlService RedirectUrlService => GetRequiredService<IRedirectUrlService>();

        public override void CreateTestData()
        {
            base.CreateTestData();

            using (IScope scope = ScopeProvider.CreateScope())
            {
                var repository = new RedirectUrlRepository((IScopeAccessor)ScopeProvider, AppCaches.Disabled, Mock.Of<ILogger<RedirectUrlRepository>>());
                IContent rootContent = ContentService.GetRootContent().First();
                var subPages = ContentService.GetPagedChildren(rootContent.Id, 0, 2, out _).ToList();
                _testPage = subPages[0];
                _altTestPage = subPages[1];

                repository.Save(new RedirectUrl
                {
                    ContentKey = _testPage.Key,
                    Url = Url,
                    Culture = CultureA
                });
                repository.Save(new RedirectUrl
                {
                    ContentKey = _altTestPage.Key,
                    Url = Url,
                    Culture = CultureB
                });
                scope.Complete();
            }
        }


        [Test]
        public void Can_Get_Most_Recent_RedirectUrl()
        {
            IRedirectUrl redirect = RedirectUrlService.GetMostRecentRedirectUrl(Url);
            Assert.AreEqual(redirect.ContentId, _altTestPage.Id);
        }

        [Test]
        public void Can_Get_Most_Recent_RedirectUrl_With_Culture()
        {
            IRedirectUrl redirect = RedirectUrlService.GetMostRecentRedirectUrl(Url, CultureA);
            Assert.AreEqual(redirect.ContentId, _testPage.Id);
        }
    }
}
