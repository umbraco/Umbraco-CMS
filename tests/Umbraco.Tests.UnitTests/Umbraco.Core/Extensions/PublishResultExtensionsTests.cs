using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class PublishResultExtensionsTests
{
    /// <summary>
    ///     The mapping every <see cref="PublishResultType"/> is expected to produce. Adding a result type without
    ///     adding it here fails <see cref="Every_Result_Type_Has_An_Expected_Mapping"/>, which is deliberate: the
    ///     mapping is the only thing standing between a publish failure and a useless "unknown error" response.
    /// </summary>
    private static readonly Dictionary<PublishResultType, ContentPublishingOperationStatus> _expectedMappings = new()
    {
        [PublishResultType.SuccessPublish] = ContentPublishingOperationStatus.Success,
        [PublishResultType.SuccessPublishCulture] = ContentPublishingOperationStatus.Success,
        [PublishResultType.SuccessPublishAlready] = ContentPublishingOperationStatus.Success,
        [PublishResultType.SuccessUnpublish] = ContentPublishingOperationStatus.Success,
        [PublishResultType.SuccessUnpublishAlready] = ContentPublishingOperationStatus.Success,
        [PublishResultType.SuccessUnpublishCulture] = ContentPublishingOperationStatus.Success,
        [PublishResultType.SuccessUnpublishMandatoryCulture] = ContentPublishingOperationStatus.Success,
        [PublishResultType.SuccessUnpublishLastCulture] = ContentPublishingOperationStatus.Success,
        [PublishResultType.SuccessMixedCulture] = ContentPublishingOperationStatus.Success,
        [PublishResultType.FailedPublish] = ContentPublishingOperationStatus.Failed,
        [PublishResultType.FailedPublishPathNotPublished] = ContentPublishingOperationStatus.PathNotPublished,
        [PublishResultType.FailedPublishHasExpired] = ContentPublishingOperationStatus.HasExpired,
        [PublishResultType.FailedPublishAwaitingRelease] = ContentPublishingOperationStatus.AwaitingRelease,
        [PublishResultType.FailedPublishCultureHasExpired] = ContentPublishingOperationStatus.CultureHasExpired,
        [PublishResultType.FailedPublishCultureAwaitingRelease] = ContentPublishingOperationStatus.CultureAwaitingRelease,
        [PublishResultType.FailedPublishIsTrashed] = ContentPublishingOperationStatus.InTrash,
        [PublishResultType.FailedPublishCancelledByEvent] = ContentPublishingOperationStatus.CancelledByEvent,
        [PublishResultType.FailedPublishContentInvalid] = ContentPublishingOperationStatus.ContentInvalid,
        [PublishResultType.FailedPublishNothingToPublish] = ContentPublishingOperationStatus.NothingToPublish,
        [PublishResultType.FailedPublishMandatoryCultureMissing] = ContentPublishingOperationStatus.MandatoryCultureMissing,
        [PublishResultType.FailedPublishConcurrencyViolation] = ContentPublishingOperationStatus.ConcurrencyViolation,
        [PublishResultType.FailedPublishUnsavedChanges] = ContentPublishingOperationStatus.UnsavedChanges,
        [PublishResultType.FailedUnpublish] = ContentPublishingOperationStatus.Failed,
        [PublishResultType.FailedUnpublishCancelledByEvent] = ContentPublishingOperationStatus.CancelledByEvent,
    };

    [Test]
    public void Every_Result_Type_Has_An_Expected_Mapping()
    {
        PublishResultType[] unaccountedFor = Enum.GetValues<PublishResultType>()
            .Where(resultType => _expectedMappings.ContainsKey(resultType) is false)
            .ToArray();

        Assert.IsEmpty(
            unaccountedFor,
            $"Result types without an expected mapping: {string.Join(", ", unaccountedFor)}. Add the mapping to "
            + $"{nameof(PublishResultExtensions)} and to this test's expectations.");
    }

    [Test]
    public void Maps_Every_Result_Type_As_Expected() =>
        Assert.Multiple(() =>
        {
            foreach ((PublishResultType resultType, ContentPublishingOperationStatus expected) in _expectedMappings)
            {
                Assert.AreEqual(expected, PublishResultFor(resultType).ToContentPublishingOperationStatus(), $"for {resultType}");
            }
        });

    [Test]
    public void Never_Maps_A_Known_Result_Type_To_Unknown()
    {
        // "Unknown" is only the defensive fallback for a result type the mapping has not been taught about. Reaching it
        // for a known type would surface as the "unknown error, see the log" response the combined status exists to avoid.
        Assert.Multiple(() =>
        {
            foreach (PublishResultType resultType in Enum.GetValues<PublishResultType>())
            {
                Assert.AreNotEqual(
                    ContentPublishingOperationStatus.Unknown,
                    PublishResultFor(resultType).ToContentPublishingOperationStatus(),
                    $"for {resultType}");
            }
        });
    }

    [Test]
    public void Maps_Successes_And_Failures_Consistently_With_PublishResult_Success() =>
        // PublishResult.Success is derived from the 128 bit of the result type, and ContentEditingService branches on it
        // before mapping. The two must agree, or a failed publish could be reported as a success (or the reverse).
        Assert.Multiple(() =>
        {
            foreach (PublishResultType resultType in Enum.GetValues<PublishResultType>())
            {
                PublishResult publishResult = PublishResultFor(resultType);
                ContentPublishingOperationStatus status = publishResult.ToContentPublishingOperationStatus();

                if (publishResult.Success)
                {
                    Assert.AreEqual(ContentPublishingOperationStatus.Success, status, $"for successful {resultType}");
                }
                else
                {
                    Assert.AreNotEqual(ContentPublishingOperationStatus.Success, status, $"for failed {resultType}");
                }
            }
        });

    private static PublishResult PublishResultFor(PublishResultType resultType)
        => new(resultType, new EventMessages(), Mock.Of<IContent>());
}
