// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using Moq;
// using NUnit.Framework;
// using Umbraco.Cms.Core.Configuration.Models;
// using Umbraco.Cms.Core.Routing;
// using Umbraco.Cms.Core.Web;
// using Umbraco.Extensions;
//
// FIXME: Reintroduce if relevant
// namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;
//
// [TestFixture]
// public class ContentFinderByIdTests : ContentFinderByIdentifierTestsBase
// {
//     [SetUp]
//     public override void Setup()
//     {
//         base.Setup();
//     }
//
//     [TestCase("/1046", 1046, true)]
//     [TestCase("/1046", 1047, false)]
//     public async Task Lookup_By_Id(string urlAsString, int nodeId, bool shouldSucceed)
//     {
//         PopulateCache(nodeId, Guid.NewGuid());
//
//         var umbracoContextAccessor = GetUmbracoContextAccessor(urlAsString);
//         var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
//         var publishedRouter = CreatePublishedRouter(umbracoContextAccessor);
//         var frequest = await publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);
//         var webRoutingSettings = new WebRoutingSettings();
//         var lookup = new ContentFinderByIdPath(
//             Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == webRoutingSettings),
//             Mock.Of<ILogger<ContentFinderByIdPath>>(),
//             Mock.Of<IRequestAccessor>(),
//             umbracoContextAccessor);
//
//         var result = await lookup.TryFindContent(frequest);
//
//         Assert.AreEqual(shouldSucceed, result);
//         if (shouldSucceed)
//         {
//             Assert.AreEqual(frequest.PublishedContent!.Id, nodeId);
//         }
//         else
//         {
//             Assert.IsNull(frequest.PublishedContent);
//         }
//     }
// }
