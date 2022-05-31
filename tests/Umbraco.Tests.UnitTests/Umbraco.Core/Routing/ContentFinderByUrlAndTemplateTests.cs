using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class ContentFinderByUrlAndTemplateTests : PublishedSnapshotServiceTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();

        var xml = PublishedContentXml.BaseWebTestXml(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);
    }

    private IFileService _fileService;

    protected override ServiceContext CreateServiceContext(IContentType[] contentTypes, IMediaType[] mediaTypes, IDataType[] dataTypes)
    {
        var serviceContext = base.CreateServiceContext(contentTypes, mediaTypes, dataTypes);

        var fileService = Mock.Get(serviceContext.FileService);
        fileService.Setup(x => x.GetTemplate(It.IsAny<string>()))
            .Returns((string alias) => new Template(ShortStringHelper, alias, alias));

        _fileService = fileService.Object;

        return serviceContext;
    }

    [TestCase("/blah")]
    [TestCase("/home/Sub1/blah")]
    [TestCase("/Home/Sub1/Blah")] // different cases
    public async Task Match_Document_By_Url_With_Template(string urlAsString)
    {
        GlobalSettings.HideTopLevelNodeFromPath = false;

        var umbracoContextAccessor = GetUmbracoContextAccessor(urlAsString);
        var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
        var publishedRouter = CreatePublishedRouter(umbracoContextAccessor);
        var frequest = await publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);

        var webRoutingSettings = new WebRoutingSettings();
        var lookup = new ContentFinderByUrlAndTemplate(
            Mock.Of<ILogger<ContentFinderByUrlAndTemplate>>(),
            _fileService,
            ContentTypeService,
            umbracoContextAccessor,
            Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == webRoutingSettings));

        var result = await lookup.TryFindContent(frequest);

        var request = frequest.Build();

        Assert.IsTrue(result);
        Assert.IsNotNull(frequest.PublishedContent);
        var templateAlias = request.GetTemplateAlias();
        Assert.IsNotNull(templateAlias);
        Assert.AreEqual("blah".ToUpperInvariant(), templateAlias.ToUpperInvariant());
    }
}
