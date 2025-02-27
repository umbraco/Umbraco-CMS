// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using Moq;
// using NUnit.Framework;
// using Umbraco.Cms.Core.Configuration.Models;
// using Umbraco.Cms.Core.Routing;
// using Umbraco.Cms.Core.Web;
// using Umbraco.Extensions;
//
// namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;
//
// FIXME: Reintroduce if relevant
// [TestFixture]
// public class ContentFinderByKeyTests : ContentFinderByIdentifierTestsBase
// {
//     [SetUp]
//     public override void Setup()
//     {
//         base.Setup();
//     }
//
//     [TestCase("/1598901d-ebbe-4996-b7fb-6a6cbac13a62", "1598901d-ebbe-4996-b7fb-6a6cbac13a62", true)]
//     [TestCase("/1598901d-ebbe-4996-b7fb-6a6cbac13a62", "a383f6ed-cc54-46f1-a577-33f42e7214de", false)]
//     public async Task Lookup_By_Key(string urlAsString, string nodeKeyString, bool shouldSucceed)
//     {
//         var nodeKey = Guid.Parse(nodeKeyString);
//
//         PopulateCache(9999, nodeKey);
//
//         var umbracoContextAccessor = GetUmbracoContextAccessor(urlAsString);
//         var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
//         var publishedRouter = CreatePublishedRouter(umbracoContextAccessor);
//         var frequest = await publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);
//         var webRoutingSettings = new WebRoutingSettings();
//         var lookup = new ContentFinderByKeyPath(
//             Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == webRoutingSettings),
//             Mock.Of<ILogger<ContentFinderByKeyPath>>(),
//             Mock.Of<IRequestAccessor>(),
//             umbracoContextAccessor);
//
//         var result = await lookup.TryFindContent(frequest);
//
//         Assert.AreEqual(shouldSucceed, result);
//         if (shouldSucceed)
//         {
//             Assert.AreEqual(frequest.PublishedContent!.Key, nodeKey);
//         }
//         else
//         {
//             Assert.IsNull(frequest.PublishedContent);
//         }
//     }
// }
