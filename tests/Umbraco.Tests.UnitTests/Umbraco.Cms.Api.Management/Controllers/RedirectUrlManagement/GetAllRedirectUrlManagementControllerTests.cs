using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

[TestFixture]
public class GetAllRedirectUrlManagementControllerTests
{
    private Mock<IRedirectUrlService> _redirectUrlService = null!;
    private GetAllRedirectUrlManagementController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _redirectUrlService = new Mock<IRedirectUrlService>();
        _redirectUrlService
            .Setup(x => x.GetAllRedirectUrlsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new PagedModel<IRedirectUrl>());
        _redirectUrlService
            .Setup(x => x.SearchRedirectUrlsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new PagedModel<IRedirectUrl>());

        var presentationFactory = new Mock<IRedirectUrlPresentationFactory>();
        presentationFactory
            .Setup(x => x.CreateMany(It.IsAny<IEnumerable<IRedirectUrl>>()))
            .Returns([]);

        _controller = new GetAllRedirectUrlManagementController(
            _redirectUrlService.Object,
            presentationFactory.Object);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task Get_All_Without_A_Filter_Term_Does_Not_Search(string? filter)
    {
        await _controller.GetAll(CancellationToken.None, filter);

        _redirectUrlService.Verify(x => x.GetAllRedirectUrlsAsync(0, 100), Times.Once);
        _redirectUrlService.Verify(
            x => x.SearchRedirectUrlsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [Test]
    public async Task Get_All_With_A_Filter_Term_Searches()
    {
        await _controller.GetAll(CancellationToken.None, "term");

        _redirectUrlService.Verify(x => x.SearchRedirectUrlsAsync("term", 0, 100), Times.Once);
        _redirectUrlService.Verify(
            x => x.GetAllRedirectUrlsAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }
}
