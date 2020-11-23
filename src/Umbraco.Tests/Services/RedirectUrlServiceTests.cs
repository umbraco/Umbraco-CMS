using System;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class RedirectUrlServiceTests : TestWithSomeContentBase
    {
        private IContent _testPage;
        private IContent _altTestPage;
        private string _url = "blah";
        private string _cultureA = "en";
        private string _cultureB = "de";
        public override void CreateTestData()
        {
            base.CreateTestData();

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new RedirectUrlRepository((IScopeAccessor)provider, AppCaches.Disabled, Mock.Of<ILogger>());
                var rootContent = ServiceContext.ContentService.GetRootContent().FirstOrDefault();
                var subPages = ServiceContext.ContentService.GetPagedChildren(rootContent.Id, 0, 2, out _).ToList();
                _testPage = subPages[0];
                _altTestPage = subPages[1];

                repository.Save(new RedirectUrl
                {
                    ContentKey = _testPage.Key,
                    Url = _url,
                    Culture = _cultureA
                });
                repository.Save(new RedirectUrl
                {
                    ContentKey = _altTestPage.Key,
                    Url = _url,
                    Culture = _cultureB
                });
                scope.Complete();
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Can_Get_Most_Recent_RedirectUrl()
        {
            var redirectUrlService = ServiceContext.RedirectUrlService;
            var redirect = redirectUrlService.GetMostRecentRedirectUrl(_url);
            Assert.AreEqual(redirect.ContentId, _altTestPage.Id);

        }

        [Test]
        public void Can_Get_Most_Recent_RedirectUrl_With_Culture()
        {
            var redirectUrlService = ServiceContext.RedirectUrlService;
            var redirect = redirectUrlService.GetMostRecentRedirectUrl(_url, _cultureA);
            Assert.AreEqual(redirect.ContentId, _testPage.Id);

        }

    }
}
