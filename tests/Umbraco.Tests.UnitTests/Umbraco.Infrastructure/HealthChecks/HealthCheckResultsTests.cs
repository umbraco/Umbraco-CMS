// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Cms.Core.HealthChecks;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HealthChecks;

/// <summary>
/// Provides unit tests for verifying the behavior and results of the HealthCheckResults class in the Umbraco infrastructure.
/// </summary>
[TestFixture]
public class HealthCheckResultsTests
{
    /// <summary>
    /// A stub implementation of a health check for testing purposes.
    /// </summary>
    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A1", "Stub check")]
    public abstract class StubHealthCheck : HealthCheck
    {
        private readonly string _message;
        private readonly StatusResultType _resultType;

    /// <summary>
    /// Initializes a new instance of the <see cref="StubHealthCheck"/> class with the specified result type and message.
    /// </summary>
    /// <param name="resultType">The status result type of the health check.</param>
    /// <param name="message">The message associated with the health check result.</param>
        public StubHealthCheck(StatusResultType resultType, string message)
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
            Task.FromResult<IEnumerable<HealthCheckStatus>>(new List<HealthCheckStatus> { new(_message) { ResultType = _resultType } });
    }

    /// <summary>
    /// A stub health check used for testing purposes.
    /// </summary>
    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A1", "Stub check 1")]
    public class StubHealthCheck1 : StubHealthCheck
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="StubHealthCheck1"/> class.
    /// </summary>
    /// <param name="resultType">The result type of the health check.</param>
    /// <param name="message">The message associated with the health check result.</param>
        public StubHealthCheck1(StatusResultType resultType, string message)
            : base(resultType, message)
        {
        }
    }

    /// <summary>
    /// A stub health check used for testing purposes.
    /// </summary>
    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A2", "Stub check 2")]
    public class StubHealthCheck2 : StubHealthCheck
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="StubHealthCheck2"/> class.
    /// </summary>
    /// <param name="resultType">The type of the status result.</param>
    /// <param name="message">The message associated with the status result.</param>
        public StubHealthCheck2(StatusResultType resultType, string message)
            : base(resultType, message)
        {
        }
    }

    /// <summary>
    /// A stub health check used for testing purposes.
    /// </summary>
    [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A3", "Stub check 3")]
    public class StubHealthCheck3 : StubHealthCheck
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="StubHealthCheck3"/> class with the specified result type and message.
    /// </summary>
    /// <param name="resultType">The type of the status result.</param>
    /// <param name="message">The message associated with the health check result.</param>
        public StubHealthCheck3(StatusResultType resultType, string message)
            : base(resultType, message)
        {
        }

    /// <summary>
    /// Gets the status asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of HealthCheckStatus.</returns>
        public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync()
            => throw new Exception("Check threw exception");
    }

    /// <summary>
    /// Tests that when all health checks are successful, the result description correctly reflects the success.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that when health checks include failing checks, the resulting description correctly reflects the success and error statuses.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that when health checks contain errors, the resulting description correctly reflects the error states.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that the health check results with summary verbosity return the correct result description.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that the health check results with detailed verbosity return the correct result description.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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
