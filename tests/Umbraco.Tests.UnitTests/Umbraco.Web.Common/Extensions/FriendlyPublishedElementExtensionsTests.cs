// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class FriendlyPublishedElementExtensionsTests
{
    private Mock<IVariationContextAccessor> _variationContextAccessor = null!;
    private Mock<IUserService> _userService = null!;
    private Mock<IPublishedValueFallback> _publishedValueFallback = null!;
    private IServiceProvider _originalServiceProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _originalServiceProvider = StaticServiceProvider.Instance;

        _variationContextAccessor = new Mock<IVariationContextAccessor>(MockBehavior.Strict);
        _userService = new Mock<IUserService>(MockBehavior.Strict);
        _publishedValueFallback = new Mock<IPublishedValueFallback>(MockBehavior.Strict);

        var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        serviceProvider.Setup(s => s.GetService(typeof(IVariationContextAccessor))).Returns(_variationContextAccessor.Object);
        serviceProvider.Setup(s => s.GetService(typeof(IUserService))).Returns(_userService.Object);
        serviceProvider.Setup(s => s.GetService(typeof(IPublishedValueFallback))).Returns(_publishedValueFallback.Object);

        StaticServiceProvider.Instance = serviceProvider.Object;
        FriendlyPublishedElementExtensions.Reset();
    }

    [TearDown]
    public void TearDown()
    {
        FriendlyPublishedElementExtensions.Reset();
        StaticServiceProvider.Instance = _originalServiceProvider;
    }

    [Test]
    public void Name_Returns_Invariant_Name()
    {
        var contentType = new Mock<IPublishedContentType>(MockBehavior.Strict);
        contentType.Setup(x => x.Variations).Returns(ContentVariation.Nothing);

        var element = new Mock<IPublishedElement>(MockBehavior.Strict);
        element.Setup(x => x.ContentType).Returns(contentType.Object);
        element.Setup(x => x.Cultures).Returns(
            new Dictionary<string, PublishedCultureInfo>
            {
                { string.Empty, new PublishedCultureInfo(string.Empty, "Home Page", "home-page", DateTime.Now) },
            });

        Assert.That(element.Object.Name(), Is.EqualTo("Home Page"));
    }

    [Test]
    public void Name_Returns_Variant_Name_From_Context()
    {
        _variationContextAccessor.Setup(x => x.VariationContext).Returns(new VariationContext("da-DK"));

        var contentType = new Mock<IPublishedContentType>(MockBehavior.Strict);
        contentType.Setup(x => x.Variations).Returns(ContentVariation.Culture);

        var element = new Mock<IPublishedElement>(MockBehavior.Strict);
        element.Setup(x => x.ContentType).Returns(contentType.Object);
        element.Setup(x => x.Cultures).Returns(
            new Dictionary<string, PublishedCultureInfo>
            {
                { "en-US", new PublishedCultureInfo("en-US", "Home", "home", DateTime.Now) },
                { "da-DK", new PublishedCultureInfo("da-DK", "Hjem", "hjem", DateTime.Now) },
            });

        Assert.That(element.Object.Name(), Is.EqualTo("Hjem"));
    }

    [Test]
    public void Name_Returns_Name_For_Explicit_Culture()
    {
        var contentType = new Mock<IPublishedContentType>(MockBehavior.Strict);
        contentType.Setup(x => x.Variations).Returns(ContentVariation.Culture);

        var element = new Mock<IPublishedElement>(MockBehavior.Strict);
        element.Setup(x => x.ContentType).Returns(contentType.Object);
        element.Setup(x => x.Cultures).Returns(
            new Dictionary<string, PublishedCultureInfo>
            {
                { "en-US", new PublishedCultureInfo("en-US", "Home", "home", DateTime.Now) },
                { "da-DK", new PublishedCultureInfo("da-DK", "Hjem", "hjem", DateTime.Now) },
            });

        Assert.That(element.Object.Name("en-US"), Is.EqualTo("Home"));
    }

    [Test]
    public void CultureDate_Returns_UpdateDate_When_Invariant()
    {
        var updateDate = new DateTime(2025, 6, 15);

        var contentType = new Mock<IPublishedContentType>(MockBehavior.Strict);
        contentType.Setup(x => x.Variations).Returns(ContentVariation.Nothing);

        var element = new Mock<IPublishedElement>(MockBehavior.Strict);
        element.Setup(x => x.ContentType).Returns(contentType.Object);
        element.Setup(x => x.UpdateDate).Returns(updateDate);

        Assert.That(element.Object.CultureDate(), Is.EqualTo(updateDate));
    }

    [Test]
    public void CultureDate_Returns_Culture_Date_For_Explicit_Culture()
    {
        var cultureDate = new DateTime(2025, 7, 20);

        var contentType = new Mock<IPublishedContentType>(MockBehavior.Strict);
        contentType.Setup(x => x.Variations).Returns(ContentVariation.Culture);

        var element = new Mock<IPublishedElement>(MockBehavior.Strict);
        element.Setup(x => x.ContentType).Returns(contentType.Object);
        element.Setup(x => x.Cultures).Returns(
            new Dictionary<string, PublishedCultureInfo>
            {
                { "en-US", new PublishedCultureInfo("en-US", "Home", "home", cultureDate) },
            });

        Assert.That(element.Object.CultureDate("en-US"), Is.EqualTo(cultureDate));
    }

    [Test]
    public void CreatorName_Returns_Name_When_User_Found()
    {
        var profile = new Mock<IProfile>(MockBehavior.Strict);
        profile.Setup(x => x.Name).Returns("Admin");
        _userService.Setup(x => x.GetProfileById(10)).Returns(profile.Object);

        var element = new Mock<IPublishedElement>(MockBehavior.Strict);
        element.Setup(x => x.CreatorId).Returns(10);

        Assert.That(element.Object.CreatorName(), Is.EqualTo("Admin"));
    }

    [Test]
    public void CreatorName_Returns_Null_When_User_Not_Found()
    {
        _userService.Setup(x => x.GetProfileById(99)).Returns((IProfile?)null);

        var element = new Mock<IPublishedElement>(MockBehavior.Strict);
        element.Setup(x => x.CreatorId).Returns(99);

        Assert.That(element.Object.CreatorName(), Is.Null);
    }

    [Test]
    public void WriterName_Returns_Name_When_User_Found()
    {
        var profile = new Mock<IProfile>(MockBehavior.Strict);
        profile.Setup(x => x.Name).Returns("Editor");
        _userService.Setup(x => x.GetProfileById(20)).Returns(profile.Object);

        var element = new Mock<IPublishedElement>(MockBehavior.Strict);
        element.Setup(x => x.WriterId).Returns(20);

        Assert.That(element.Object.WriterName(), Is.EqualTo("Editor"));
    }

    [Test]
    public void WriterName_Returns_Null_When_User_Not_Found()
    {
        _userService.Setup(x => x.GetProfileById(99)).Returns((IProfile?)null);

        var element = new Mock<IPublishedElement>(MockBehavior.Strict);
        element.Setup(x => x.WriterId).Returns(99);

        Assert.That(element.Object.WriterName(), Is.Null);
    }
}
