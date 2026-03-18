// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Infrastructure.HealthChecks;
using Umbraco.Cms.Infrastructure.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HealthChecks;

/// <summary>
/// Unit tests for the MarkdownToHtmlConverter class.
/// </summary>
[TestFixture]
public class MarkdownToHtmlConverterTests
{
    /// <summary>
    /// Tests that the ToHtml method applies green highlighting for success results.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ToHtml_WithSuccessResult_AppliesGreenHighlighting()
    {
        var checks = new List<HealthCheck> { new SuccessHealthCheck() };
        var results = await HealthCheckResults.Create(checks);
        var converter = CreateConverter();

        var html = converter.ToHtml(results, HealthCheckNotificationVerbosity.Summary);

        Assert.That(html, Does.Contain("<span style=\"color: #5cb85c\">Success</span>"));
    }

    /// <summary>
    /// Tests that the HTML output applies orange highlighting for warning results.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ToHtml_WithWarningResult_AppliesOrangeHighlighting()
    {
        var checks = new List<HealthCheck> { new WarningHealthCheck() };
        var results = await HealthCheckResults.Create(checks);
        var converter = CreateConverter();

        var html = converter.ToHtml(results, HealthCheckNotificationVerbosity.Summary);

        Assert.That(html, Does.Contain("<span style=\"color: #f0ad4e\">Warning</span>"));
    }

    /// <summary>
    /// Verifies that the <c>ToHtml</c> method applies red highlighting to error results in the generated HTML output.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task ToHtml_WithErrorResult_AppliesRedHighlighting()
    {
        var checks = new List<HealthCheck> { new ErrorHealthCheck() };
        var results = await HealthCheckResults.Create(checks);
        var converter = CreateConverter();

        var html = converter.ToHtml(results, HealthCheckNotificationVerbosity.Summary);

        Assert.That(html, Does.Contain("<span style=\"color: #d9534f\">Error</span>"));
    }

    /// <summary>
    /// Tests that the <c>ToHtml</c> method applies the correct color highlighting for each health check status (Success, Warning, Error) when provided with mixed results.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task ToHtml_WithMixedResults_AppliesCorrectHighlightingForEachStatus()
    {
        var checks = new List<HealthCheck>
        {
            new SuccessHealthCheck(),
            new WarningHealthCheck(),
            new ErrorHealthCheck(),
        };
        var results = await HealthCheckResults.Create(checks);
        var converter = CreateConverter();

        var html = converter.ToHtml(results, HealthCheckNotificationVerbosity.Summary);

        Assert.Multiple(() =>
        {
            Assert.That(html, Does.Contain("<span style=\"color: #5cb85c\">Success</span>"), "Success should have green highlighting");
            Assert.That(html, Does.Contain("<span style=\"color: #f0ad4e\">Warning</span>"), "Warning should have orange highlighting");
            Assert.That(html, Does.Contain("<span style=\"color: #d9534f\">Error</span>"), "Error should have red highlighting");
        });
    }

    /// <summary>
    /// Tests that the ToHtml method with detailed verbosity includes the message in the output.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ToHtml_WithDetailedVerbosity_IncludesMessageInOutput()
    {
        var checks = new List<HealthCheck> { new SuccessHealthCheck() };
        var results = await HealthCheckResults.Create(checks);
        var converter = CreateConverter();

        var html = converter.ToHtml(results, HealthCheckNotificationVerbosity.Detailed);

        Assert.That(html, Does.Contain("Check passed"));
    }

    /// <summary>
    /// Tests that the HTML output generated with summary verbosity does not include the success message.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ToHtml_WithSummaryVerbosity_DoesNotIncludeSuccessMessageInOutput()
    {
        var checks = new List<HealthCheck> { new SuccessHealthCheck() };
        var results = await HealthCheckResults.Create(checks);
        var converter = CreateConverter();

        var html = converter.ToHtml(results, HealthCheckNotificationVerbosity.Summary);

        Assert.That(html, Does.Not.Contain("Check passed"));
    }

    /// <summary>
    /// A stub health check implementation used for testing purposes.
    /// </summary>
    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A1", "Stub check")]
    public abstract class StubHealthCheck : HealthCheck
    {
        private readonly string _message;
        private readonly StatusResultType _resultType;

        protected StubHealthCheck(StatusResultType resultType, string message)
        {
            _resultType = resultType;
            _message = message;
        }

    /// <summary>
    /// Executes the specified health check action.
    /// </summary>
    /// <param name="action">The health check action to execute.</param>
    /// <returns>The status result of the health check action.</returns>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action) =>
            throw new NotImplementedException();

    /// <summary>
    /// Gets the status asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of HealthCheckStatus.</returns>
        public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync() =>
            Task.FromResult<IEnumerable<HealthCheckStatus>>([new(_message) { ResultType = _resultType }]);
    }

    /// <summary>
    /// Tests that the health check for the Markdown to HTML converter returns a successful result when expected.
    /// </summary>
    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A1", "Success Check")]
    public class SuccessHealthCheck : StubHealthCheck
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="SuccessHealthCheck"/> class.
    /// </summary>
        public SuccessHealthCheck()
            : base(StatusResultType.Success, "Check passed")
        {
        }
    }

    /// <summary>
    /// A test health check class used to simulate a warning status in health check scenarios.
    /// </summary>
    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A2", "Warning Check")]
    public class WarningHealthCheck : StubHealthCheck
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="WarningHealthCheck"/> class.
    /// </summary>
        public WarningHealthCheck()
            : base(StatusResultType.Warning, "Check has warnings")
        {
        }
    }

    /// <summary>
    /// Tests the health check functionality when an error occurs during Markdown to HTML conversion.
    /// Ensures that the health check correctly identifies and reports conversion errors.
    /// </summary>
    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A3", "Error Check")]
    public class ErrorHealthCheck : StubHealthCheck
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorHealthCheck"/> class.
    /// </summary>
        public ErrorHealthCheck()
            : base(StatusResultType.Error, "Check failed")
        {
        }
    }

#pragma warning disable CS0618 // Type or member is obsolete
    private static MarkdownToHtmlConverter CreateConverter() =>
        new(new HeyRedMarkdownToHtmlConverter());
#pragma warning restore CS0618 // Type or member is obsolete
}
