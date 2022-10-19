// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Cms.Core.HealthChecks;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HealthChecks;

[TestFixture]
public class HealthCheckResultsTests
{
    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A1", "Stub check")]
    public abstract class StubHealthCheck : HealthCheck
    {
        private readonly string _message;
        private readonly StatusResultType _resultType;

        public StubHealthCheck(StatusResultType resultType, string message)
        {
            _resultType = resultType;
            _message = message;
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action) =>
            throw new NotImplementedException();

        public override async Task<IEnumerable<HealthCheckStatus>> GetStatus() =>
            new List<HealthCheckStatus> { new(_message) { ResultType = _resultType } };
    }

    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A1", "Stub check 1")]
    public class StubHealthCheck1 : StubHealthCheck
    {
        public StubHealthCheck1(StatusResultType resultType, string message)
            : base(resultType, message)
        {
        }
    }

    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A2", "Stub check 2")]
    public class StubHealthCheck2 : StubHealthCheck
    {
        public StubHealthCheck2(StatusResultType resultType, string message)
            : base(resultType, message)
        {
        }
    }

    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A3", "Stub check 3")]
    public class StubHealthCheck3 : StubHealthCheck
    {
        public StubHealthCheck3(StatusResultType resultType, string message)
            : base(resultType, message)
        {
        }

        public override async Task<IEnumerable<HealthCheckStatus>> GetStatus() =>
            throw new Exception("Check threw exception");
    }

    [Test]
    public async Task HealthCheckResults_WithSuccessfulChecks_ReturnsCorrectResultDescription()
    {
        var checks = new List<HealthCheck>
        {
            new StubHealthCheck1(StatusResultType.Success, "First check was successful"),
            new StubHealthCheck2(StatusResultType.Success, "Second check was successful"),
        };
        var results = await HealthCheckResults.Create(checks);

        Assert.IsTrue(results.AllChecksSuccessful);

        var resultAsMarkdown = results.ResultsAsMarkDown(HealthCheckNotificationVerbosity.Summary);
        Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 1' all completed successfully.") > -1);
        Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 2' all completed successfully.") > -1);
    }

    [Test]
    public async Task HealthCheckResults_WithFailingChecks_ReturnsCorrectResultDescription()
    {
        var checks = new List<HealthCheck>
        {
            new StubHealthCheck1(StatusResultType.Success, "First check was successful"),
            new StubHealthCheck2(StatusResultType.Error, "Second check was not successful"),
        };
        var results = await HealthCheckResults.Create(checks);

        Assert.IsFalse(results.AllChecksSuccessful);

        var resultAsMarkdown = results.ResultsAsMarkDown(HealthCheckNotificationVerbosity.Summary);
        Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 1' all completed successfully.") > -1);
        Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 2' completed with errors.") > -1);
    }

    [Test]
    public async Task HealthCheckResults_WithErroringCheck_ReturnsCorrectResultDescription()
    {
        var checks = new List<HealthCheck>
        {
            new StubHealthCheck1(StatusResultType.Success, "First check was successful"),
            new StubHealthCheck3(StatusResultType.Error, "Third check was not successful"),
            new StubHealthCheck2(StatusResultType.Error, "Second check was not successful"),
        };
        var results = await HealthCheckResults.Create(checks);

        Assert.IsFalse(results.AllChecksSuccessful);

        var resultAsMarkdown = results.ResultsAsMarkDown(HealthCheckNotificationVerbosity.Summary);
        Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 1' all completed successfully.") > -1);
        Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 2' completed with errors.") > -1);
        Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 3' completed with errors.") > -1);
    }

    [Test]
    public async Task HealthCheckResults_WithSummaryVerbosity_ReturnsCorrectResultDescription()
    {
        var checks = new List<HealthCheck>
        {
            new StubHealthCheck1(StatusResultType.Success, "First check was successful"),
            new StubHealthCheck2(StatusResultType.Success, "Second check was successful"),
        };
        var results = await HealthCheckResults.Create(checks);

        var resultAsMarkdown = results.ResultsAsMarkDown(HealthCheckNotificationVerbosity.Summary);
        Assert.IsTrue(resultAsMarkdown.IndexOf("Result: 'Success'" + Environment.NewLine) > -1);
    }

    [Test]
    public async Task HealthCheckResults_WithDetailedVerbosity_ReturnsCorrectResultDescription()
    {
        var checks = new List<HealthCheck>
        {
            new StubHealthCheck1(StatusResultType.Success, "First check was successful"),
            new StubHealthCheck2(StatusResultType.Success, "Second check was successful"),
        };
        var results = await HealthCheckResults.Create(checks);

        var resultAsMarkdown = results.ResultsAsMarkDown(HealthCheckNotificationVerbosity.Detailed);
        Assert.IsFalse(resultAsMarkdown.IndexOf("Result: 'Success'" + Environment.NewLine) > -1);
        Assert.IsTrue(resultAsMarkdown.IndexOf("Result: 'Success', Message: 'First check was successful'" +
                                               Environment.NewLine) > -1);
    }
}
