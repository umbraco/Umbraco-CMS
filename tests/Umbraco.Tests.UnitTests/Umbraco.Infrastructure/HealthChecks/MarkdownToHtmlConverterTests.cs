// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Infrastructure.HealthChecks;
using Umbraco.Cms.Infrastructure.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HealthChecks;

[TestFixture]
public class MarkdownToHtmlConverterTests
{
    [Test]
    public async Task ToHtml_WithSuccessResult_AppliesGreenHighlighting()
    {
        var checks = new List<HealthCheck> { new SuccessHealthCheck() };
        var results = await HealthCheckResults.Create(checks);
        var converter = CreateConverter();

        var html = converter.ToHtml(results, HealthCheckNotificationVerbosity.Summary);

        Assert.That(html, Does.Contain("<span style=\"color: #5cb85c\">Success</span>"));
    }

    [Test]
    public async Task ToHtml_WithWarningResult_AppliesOrangeHighlighting()
    {
        var checks = new List<HealthCheck> { new WarningHealthCheck() };
        var results = await HealthCheckResults.Create(checks);
        var converter = CreateConverter();

        var html = converter.ToHtml(results, HealthCheckNotificationVerbosity.Summary);

        Assert.That(html, Does.Contain("<span style=\"color: #f0ad4e\">Warning</span>"));
    }

    [Test]
    public async Task ToHtml_WithErrorResult_AppliesRedHighlighting()
    {
        var checks = new List<HealthCheck> { new ErrorHealthCheck() };
        var results = await HealthCheckResults.Create(checks);
        var converter = CreateConverter();

        var html = converter.ToHtml(results, HealthCheckNotificationVerbosity.Summary);

        Assert.That(html, Does.Contain("<span style=\"color: #d9534f\">Error</span>"));
    }

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

    [Test]
    public async Task ToHtml_WithDetailedVerbosity_IncludesMessageInOutput()
    {
        var checks = new List<HealthCheck> { new SuccessHealthCheck() };
        var results = await HealthCheckResults.Create(checks);
        var converter = CreateConverter();

        var html = converter.ToHtml(results, HealthCheckNotificationVerbosity.Detailed);

        Assert.That(html, Does.Contain("Check passed"));
    }

    [Test]
    public async Task ToHtml_WithSummaryVerbosity_DoesNotIncludeSuccessMessageInOutput()
    {
        var checks = new List<HealthCheck> { new SuccessHealthCheck() };
        var results = await HealthCheckResults.Create(checks);
        var converter = CreateConverter();

        var html = converter.ToHtml(results, HealthCheckNotificationVerbosity.Summary);

        Assert.That(html, Does.Not.Contain("Check passed"));
    }

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

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action) =>
            throw new NotImplementedException();

        public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync() =>
            Task.FromResult<IEnumerable<HealthCheckStatus>>([new(_message) { ResultType = _resultType }]);
    }

    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A1", "Success Check")]
    public class SuccessHealthCheck : StubHealthCheck
    {
        public SuccessHealthCheck()
            : base(StatusResultType.Success, "Check passed")
        {
        }
    }

    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A2", "Warning Check")]
    public class WarningHealthCheck : StubHealthCheck
    {
        public WarningHealthCheck()
            : base(StatusResultType.Warning, "Check has warnings")
        {
        }
    }

    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A3", "Error Check")]
    public class ErrorHealthCheck : StubHealthCheck
    {
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
