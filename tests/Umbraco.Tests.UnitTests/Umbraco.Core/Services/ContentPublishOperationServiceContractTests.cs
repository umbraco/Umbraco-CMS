using System.ComponentModel;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

/// <summary>
/// Contract tests for IContentPublishOperationService interface.
/// These tests verify interface design and expected behaviors.
/// </summary>
[TestFixture]
public class ContentPublishOperationServiceContractTests
{
    [Test]
    public void IContentPublishOperationService_Inherits_From_IService()
    {
        // Assert
        Assert.That(typeof(IService).IsAssignableFrom(typeof(IContentPublishOperationService)), Is.True);
    }

    [Test]
    public void Publish_Method_Exists_With_Expected_Signature()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.Publish),
            new[] { typeof(IContent), typeof(string[]), typeof(int) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(PublishResult)));
    }

    [Test]
    public void Unpublish_Method_Exists_With_Expected_Signature()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.Unpublish),
            new[] { typeof(IContent), typeof(string), typeof(int) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(PublishResult)));
    }

    [Test]
    public void PublishBranch_Method_Exists_With_Expected_Signature()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.PublishBranch),
            new[] { typeof(IContent), typeof(PublishBranchFilter), typeof(string[]), typeof(int) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(IEnumerable<PublishResult>)));
    }

    [Test]
    public void PerformScheduledPublish_Method_Exists_With_Expected_Signature()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.PerformScheduledPublish),
            new[] { typeof(DateTime) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(IEnumerable<PublishResult>)));
    }

    [Test]
    public void GetContentScheduleByContentId_IntOverload_Exists()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.GetContentScheduleByContentId),
            new[] { typeof(int) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(ContentScheduleCollection)));
    }

    [Test]
    public void GetContentScheduleByContentId_GuidOverload_Exists()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.GetContentScheduleByContentId),
            new[] { typeof(Guid) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(ContentScheduleCollection)));
    }

    [Test]
    public void IsPathPublishable_Method_Exists()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.IsPathPublishable),
            new[] { typeof(IContent) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(bool)));
    }

    [Test]
    public void IsPathPublished_Method_Exists()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.IsPathPublished),
            new[] { typeof(IContent) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(bool)));
    }

    [Test]
    public void SendToPublication_Method_Exists()
    {
        // Arrange
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.SendToPublication),
            new[] { typeof(IContent), typeof(int) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(bool)));
    }

    [Test]
    public void CommitDocumentChanges_Method_Exists_With_NotificationState_Parameter()
    {
        // Arrange - Critical Review Option A: Exposed for orchestration
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.CommitDocumentChanges),
            new[] { typeof(IContent), typeof(int), typeof(IDictionary<string, object?>) });

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(methodInfo!.ReturnType, Is.EqualTo(typeof(PublishResult)));
    }

    [Test]
    public void CommitDocumentChanges_Has_EditorBrowsable_Advanced_Attribute()
    {
        // Arrange - Should be hidden from IntelliSense by default
        var methodInfo = typeof(IContentPublishOperationService).GetMethod(
            nameof(IContentPublishOperationService.CommitDocumentChanges),
            new[] { typeof(IContent), typeof(int), typeof(IDictionary<string, object?>) });

        // Act
        var attribute = methodInfo?.GetCustomAttributes(typeof(EditorBrowsableAttribute), false)
            .Cast<EditorBrowsableAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute!.State, Is.EqualTo(EditorBrowsableState.Advanced));
    }
}
