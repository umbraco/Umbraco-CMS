using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Events;

[TestFixture]
public class AddUnroutableContentWarningsWhenPublishingNotificationHandlerTests
{
    private const string Header = "unroutableContentWarningHeader";
    private const string WarningWithoutReason = "unroutableContentWarning";
    private const string WarningWithReason = "unroutableContentWarningWithReason";

    private EventMessages _eventMessages = null!;
    private Mock<IPublishedUrlInfoProvider> _publishedUrlInfoProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _eventMessages = new EventMessages();
        _publishedUrlInfoProvider = new Mock<IPublishedUrlInfoProvider>();
    }

    [Test]
    public async Task Cannot_Warn_When_Warnings_Are_Disabled()
    {
        IContent content = CreateInvariantContent();
        SetupUrls(content, UrlInfo.AsMessage("cannot route", Constants.UrlProviders.Content, "en-US"));

        await CreateHandler(showWarnings: false).HandleAsync(CreateNotification(content), CancellationToken.None);

        Assert.That(_eventMessages.Count, Is.Zero);
        _publishedUrlInfoProvider.Verify(x => x.GetAllAsync(It.IsAny<IContent>()), Times.Never);
    }

    [Test]
    public async Task Cannot_Warn_When_Content_Has_A_Url()
    {
        IContent content = CreateInvariantContent();
        SetupUrls(content, UrlInfo.AsUrl("/about/", Constants.UrlProviders.Content, "en-US"));

        await CreateHandler().HandleAsync(CreateNotification(content), CancellationToken.None);

        Assert.That(_eventMessages.Count, Is.Zero);
    }

    [Test]
    public async Task Can_Warn_With_The_Reported_Reason_When_Invariant_Content_Has_No_Url()
    {
        IContent content = CreateInvariantContent();
        SetupUrls(content, UrlInfo.AsMessage("URL cannot be routed", Constants.UrlProviders.Content, "en-US"));

        await CreateHandler().HandleAsync(CreateNotification(content), CancellationToken.None);

        EventMessage message = _eventMessages.GetAll().Single();
        Assert.Multiple(() =>
        {
            Assert.That(message.MessageType, Is.EqualTo(EventMessageType.Warning));
            Assert.That(message.Category, Is.EqualTo(Header));
            Assert.That(message.Message, Is.EqualTo($"{WarningWithReason}:URL cannot be routed."));
        });
    }

    [Test]
    public async Task Can_Combine_Distinct_Reasons_Into_A_Single_Warning()
    {
        IContent content = CreateInvariantContent();
        SetupUrls(
            content,
            UrlInfo.AsMessage("URL cannot be routed", Constants.UrlProviders.Content, "en-US"),
            UrlInfo.AsMessage("URL cannot be routed", Constants.UrlProviders.Content, "da-DK"),
            UrlInfo.AsMessage("URL would collide.", Constants.UrlProviders.Content, "de-DE"));

        await CreateHandler().HandleAsync(CreateNotification(content), CancellationToken.None);

        EventMessage message = _eventMessages.GetAll().Single();
        Assert.That(message.Message, Is.EqualTo($"{WarningWithReason}:URL cannot be routed. URL would collide."));
    }

    [Test]
    public async Task Can_Warn_Without_A_Reason_When_None_Is_Reported()
    {
        IContent content = CreateInvariantContent();
        SetupUrls(content);

        await CreateHandler().HandleAsync(CreateNotification(content), CancellationToken.None);

        EventMessage message = _eventMessages.GetAll().Single();
        Assert.That(message.Message, Is.EqualTo(WarningWithoutReason));
    }

    [Test]
    public async Task Can_Warn_Using_Only_The_Published_Cultures_Of_Variant_Content()
    {
        IContent content = CreateVariantContent("da-DK");
        SetupUrls(
            content,
            UrlInfo.AsUrl("/", Constants.UrlProviders.Content, "en-US"),
            UrlInfo.AsMessage("Could not get the URL", Constants.UrlProviders.Content, "da-dk"));

        await CreateHandler().HandleAsync(CreateNotification(content), CancellationToken.None);

        EventMessage message = _eventMessages.GetAll().Single();
        Assert.That(message.Message, Is.EqualTo($"{WarningWithReason}:Could not get the URL."));
    }

    [Test]
    public async Task Cannot_Warn_When_Every_Published_Culture_Of_Variant_Content_Has_A_Url()
    {
        IContent content = CreateVariantContent("en-US");
        SetupUrls(
            content,
            UrlInfo.AsUrl("/", Constants.UrlProviders.Content, "en-US"),
            UrlInfo.AsMessage("Could not get the URL", Constants.UrlProviders.Content, "da-DK"));

        await CreateHandler().HandleAsync(CreateNotification(content), CancellationToken.None);

        Assert.That(_eventMessages.Count, Is.Zero);
    }

    [Test]
    public async Task Can_Warn_Once_Per_Document_Even_When_Several_Cultures_Have_No_Url()
    {
        IContent content = CreateVariantContent("en-US", "da-DK");
        SetupUrls(
            content,
            UrlInfo.AsMessage("Could not get the URL", Constants.UrlProviders.Content, "en-US"),
            UrlInfo.AsMessage("Could not get the URL", Constants.UrlProviders.Content, "da-DK"));

        await CreateHandler().HandleAsync(CreateNotification(content), CancellationToken.None);

        Assert.That(_eventMessages.Count, Is.EqualTo(1));
    }

    private AddUnroutableContentWarningsWhenPublishingNotificationHandler CreateHandler(bool showWarnings = true)
    {
        IUmbracoContext umbracoContext = Mock.Of<IUmbracoContext>();
        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        umbracoContextAccessor.Setup(x => x.TryGetUmbracoContext(out umbracoContext)).Returns(true);

        var eventMessagesFactory = new Mock<IEventMessagesFactory>();
        eventMessagesFactory.Setup(x => x.Get()).Returns(_eventMessages);

        // Echo the alias, and the first token if any, so the assertions can see which text was chosen.
        var localizedTextService = new Mock<ILocalizedTextService>();
        localizedTextService
            .Setup(x => x.Localize("content", It.IsAny<string>(), It.IsAny<CultureInfo>(), It.IsAny<IDictionary<string, string?>?>()))
            .Returns((string _, string alias, CultureInfo _, IDictionary<string, string?>? tokens)
                => tokens is { Count: > 0 } ? $"{alias}:{tokens["0"]}" : alias);

        return new AddUnroutableContentWarningsWhenPublishingNotificationHandler(
            Mock.Of<IPublishedRouter>(),
            umbracoContextAccessor.Object,
            Mock.Of<ILanguageService>(),
            localizedTextService.Object,
            Mock.Of<IContentService>(),
            Mock.Of<IVariationContextAccessor>(),
            NullLoggerFactory.Instance,
            new UriUtility(Mock.Of<IHostingEnvironment>()),
            Mock.Of<IPublishedUrlProvider>(),
            Mock.Of<IDocumentNavigationQueryService>(),
            Mock.Of<IPublishedContentStatusFilteringService>(),
            eventMessagesFactory.Object,
            Options.Create(new ContentSettings { ShowUnroutableContentWarnings = showWarnings }),
            _publishedUrlInfoProvider.Object);
    }

    private void SetupUrls(IContent content, params UrlInfo[] urls)
        => _publishedUrlInfoProvider
            .Setup(x => x.GetAllAsync(content))
            .ReturnsAsync(new HashSet<UrlInfo>(urls));

    private static ContentPublishedNotification CreateNotification(IContent content)
        => new(content, new EventMessages());

    private static IContent CreateInvariantContent()
        => CreateContent(ContentVariation.Nothing);

    private static IContent CreateVariantContent(params string[] publishedCultures)
        => CreateContent(ContentVariation.Culture, publishedCultures);

    private static IContent CreateContent(ContentVariation variation, params string[] publishedCultures)
    {
        var contentType = new Mock<ISimpleContentType>();
        contentType.Setup(x => x.Variations).Returns(variation);

        var content = new Mock<IContent>();
        content.Setup(x => x.ContentType).Returns(contentType.Object);
        content.Setup(x => x.PublishedCultures).Returns(publishedCultures);
        return content.Object;
    }
}
