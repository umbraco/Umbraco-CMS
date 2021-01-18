using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
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
        private IContent _firstSubPage;
        private IContent _secondSubPage;
        private IContent _thirdSubPage;
        private readonly string _url = "blah";
        private readonly string _urlAlt = "alternativeUrl";
        private readonly string _cultureEnglish = "en";
        private readonly string _cultureGerman = "de";
        private readonly string _unusedCulture = "es";
        public override void CreateTestData()
        {
            base.CreateTestData();

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = new RedirectUrlRepository((IScopeAccessor)provider, AppCaches.Disabled, Mock.Of<ILogger>());
                var rootContent = ServiceContext.ContentService.GetRootContent().FirstOrDefault();
                var subPages = ServiceContext.ContentService.GetPagedChildren(rootContent.Id, 0, 3, out _).ToList();
                _firstSubPage = subPages[0];
                _secondSubPage = subPages[1];
                _thirdSubPage = subPages[2];



                repository.Save(new RedirectUrl
                {
                    ContentKey = _firstSubPage.Key,
                    Url = _url,
                    Culture = _cultureEnglish
                });
                Thread.Sleep(1000); //Added delay to ensure timestamp difference as sometimes they seem to have the same timestamp
                repository.Save(new RedirectUrl
                {
                    ContentKey = _secondSubPage.Key,
                    Url = _url,
                    Culture = _cultureGerman
                });
                Thread.Sleep(1000);
                repository.Save(new RedirectUrl
                {
                    ContentKey = _thirdSubPage.Key,
                    Url = _urlAlt,
                    Culture = string.Empty
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
            Assert.AreEqual(redirect.ContentId, _secondSubPage.Id);

        }

        [Test]
        public void Can_Get_Most_Recent_RedirectUrl_With_Culture()
        {
            var redirectUrlService = ServiceContext.RedirectUrlService;
            var redirect = redirectUrlService.GetMostRecentRedirectUrl(_url, _cultureEnglish);
            Assert.AreEqual(redirect.ContentId, _firstSubPage.Id);

        }

        [Test]
        public void Can_Get_Most_Recent_RedirectUrl_With_Culture_When_No_CultureVariant_Exists()
        {
            var redirectUrlService = ServiceContext.RedirectUrlService;
            var redirect = redirectUrlService.GetMostRecentRedirectUrl(_urlAlt, _unusedCulture);
            Assert.AreEqual(redirect.ContentId, _thirdSubPage.Id);

        }

    }
}
