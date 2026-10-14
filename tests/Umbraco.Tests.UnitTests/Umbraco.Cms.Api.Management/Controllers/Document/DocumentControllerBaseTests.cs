using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.Document;

[TestFixture]
public class DocumentControllerBaseTests
{
    private TestDocumentController _controller = null!;

    [SetUp]
    public void SetUp() => _controller = new TestDocumentController();

    [Test]
    public void Editing_Failure_Is_Reported_With_The_Editing_Status()
    {
        var status = new ContentEditingAndPublishingStatus
        {
            ContentEditingOperationStatus = ContentEditingOperationStatus.NotFound,
        };

        IActionResult result = _controller.MapStatus(status);

        ProblemDetails problemDetails = AssertProblemDetails(result, StatusCodes.Status404NotFound);
        Assert.AreEqual("The content could not be found", problemDetails.Title);
    }

    [Test]
    public void Publishing_Failure_Is_Reported_With_The_Publishing_Status()
    {
        var status = new ContentEditingAndPublishingStatus
        {
            ContentEditingOperationStatus = ContentEditingOperationStatus.Success,
            ContentPublishingOperationStatus = ContentPublishingOperationStatus.PathNotPublished,
        };

        IActionResult result = _controller.MapStatus(status);

        // this is the whole point of the combined status: the publish reason has to survive a successful save
        ProblemDetails problemDetails = AssertProblemDetails(result, StatusCodes.Status400BadRequest);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Parent not published", problemDetails.Title);
            Assert.AreEqual("Could not publish the document because its parent was not published.", problemDetails.Detail);
        });
    }

    [Test]
    public void Editing_Failure_Takes_Precedence_Over_The_Publishing_Status()
    {
        var status = new ContentEditingAndPublishingStatus
        {
            ContentEditingOperationStatus = ContentEditingOperationStatus.NotFound,
            ContentPublishingOperationStatus = ContentPublishingOperationStatus.PathNotPublished,
        };

        IActionResult result = _controller.MapStatus(status);

        ProblemDetails problemDetails = AssertProblemDetails(result, StatusCodes.Status404NotFound);
        Assert.AreEqual("The content could not be found", problemDetails.Title);
    }

    [Test]
    public void Invalid_Content_Failure_Reports_The_Invalid_Property_Aliases()
    {
        var status = new ContentEditingAndPublishingStatus
        {
            ContentEditingOperationStatus = ContentEditingOperationStatus.Success,
            ContentPublishingOperationStatus = ContentPublishingOperationStatus.ContentInvalid,
        };

        IActionResult result = _controller.MapStatus(status, ["title", "bodyText"]);

        ProblemDetails problemDetails = AssertProblemDetails(result, StatusCodes.Status400BadRequest);
        Assert.AreEqual("Invalid document", problemDetails.Title);
        Assert.IsTrue(problemDetails.Extensions.TryGetValue("invalidProperties", out var invalidProperties));
        Assert.AreEqual(new[] { "title", "bodyText" }, invalidProperties as IEnumerable<string>);
    }

    [Test]
    public void Concurrency_Violation_Is_Reported_As_A_Conflict()
    {
        // A concurrency violation is returned before the document is persisted, so it is reported against the save
        // rather than the publish - otherwise the response would state that the save succeeded.
        var status = new ContentEditingAndPublishingStatus
        {
            ContentEditingOperationStatus = ContentEditingOperationStatus.ConcurrencyViolation,
        };

        IActionResult result = _controller.MapStatus(status);

        ProblemDetails problemDetails = AssertProblemDetails(result, StatusCodes.Status409Conflict);
        Assert.AreEqual("Concurrency violation detected", problemDetails.Title);
    }

    [Test]
    public void Success_Must_Be_Handled_By_The_Controller()
    {
        var status = new ContentEditingAndPublishingStatus
        {
            ContentEditingOperationStatus = ContentEditingOperationStatus.Success,
            ContentPublishingOperationStatus = ContentPublishingOperationStatus.Success,
        };

        Assert.Throws<ArgumentException>(() => _controller.MapStatus(status));
    }

    [Test]
    public void Save_Only_Success_Must_Be_Handled_By_The_Controller()
    {
        var status = new ContentEditingAndPublishingStatus
        {
            ContentEditingOperationStatus = ContentEditingOperationStatus.Success,
        };

        Assert.Throws<ArgumentException>(() => _controller.MapStatus(status));
    }

    private static ProblemDetails AssertProblemDetails(IActionResult result, int expectedStatusCode)
    {
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(expectedStatusCode, objectResult.StatusCode);

        var problemDetails = objectResult.Value as ProblemDetails;
        Assert.IsNotNull(problemDetails);
        return problemDetails;
    }

    /// <summary>
    ///     Exposes the protected status mapping, which is where the combined operation decides between the editing
    ///     and the publishing problem details.
    /// </summary>
    private sealed class TestDocumentController : DocumentControllerBase
    {
        public IActionResult MapStatus(
            ContentEditingAndPublishingStatus status,
            IEnumerable<string>? invalidPropertyAliases = null)
            => DocumentEditingAndPublishingOperationStatusResult(status, invalidPropertyAliases);
    }
}
