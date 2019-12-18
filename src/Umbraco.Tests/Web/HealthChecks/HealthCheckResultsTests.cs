using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Web.HealthCheck;

namespace Umbraco.Tests.Web.HealthChecks
{
    [TestFixture]
    public class HealthCheckResultsTests
    {
        #region Stub checks

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

            public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<HealthCheckStatus> GetStatus()
            {
                return new List<HealthCheckStatus>
                {
                    new HealthCheckStatus(_message)
                    {
                        ResultType = _resultType
                    }
                };
            }
        }

        [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A1", "Stub check 1")]
        public class StubHealthCheck1 : StubHealthCheck
        {
            public StubHealthCheck1(StatusResultType resultType, string message) : base(resultType, message)
            {
            }
        }

        [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A2", "Stub check 2")]
        public class StubHealthCheck2 : StubHealthCheck
        {
            public StubHealthCheck2(StatusResultType resultType, string message) : base(resultType, message)
            {
            }
        }

        [HealthCheck("CFD6FC34-59C9-4402-B55F-C8BC96B628A3", "Stub check 3")]
        public class StubHealthCheck3 : StubHealthCheck
        {
            public StubHealthCheck3(StatusResultType resultType, string message) : base(resultType, message)
            {
            }

            public override IEnumerable<HealthCheckStatus> GetStatus()
            {
                throw new Exception("Check threw exception");
            }
        }

        #endregion

        [Test]
        public void HealthCheckResults_WithSuccessfulChecks_ReturnsCorrectResultDescription()
        {
            var checks = new List<HealthCheck>
            {
                new StubHealthCheck1(StatusResultType.Success, "First check was successful"),
                new StubHealthCheck2(StatusResultType.Success, "Second check was successful"),
            };
            var results = new HealthCheckResults(checks);

            Assert.IsTrue(results.AllChecksSuccessful);

            var resultAsMarkdown = results.ResultsAsMarkDown(HealthCheckNotificationVerbosity.Summary);
            Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 1' all completed successfully.") > -1);
            Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 2' all completed successfully.") > -1);
        }

        [Test]
        public void HealthCheckResults_WithFailingChecks_ReturnsCorrectResultDescription()
        {
            var checks = new List<HealthCheck>
            {
                new StubHealthCheck1(StatusResultType.Success, "First check was successful"),
                new StubHealthCheck2(StatusResultType.Error, "Second check was not successful"),
            };
            var results = new HealthCheckResults(checks);

            Assert.IsFalse(results.AllChecksSuccessful);

            var resultAsMarkdown = results.ResultsAsMarkDown(HealthCheckNotificationVerbosity.Summary);
            Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 1' all completed successfully.") > -1);
            Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 2' completed with errors.") > -1);
        }

        [Test]
        public void HealthCheckResults_WithErroringCheck_ReturnsCorrectResultDescription()
        {
            var checks = new List<HealthCheck>
            {
                new StubHealthCheck1(StatusResultType.Success, "First check was successful"),
                new StubHealthCheck3(StatusResultType.Error, "Third check was not successful"),
                new StubHealthCheck2(StatusResultType.Error, "Second check was not successful"),
            };
            var results = new HealthCheckResults(checks);

            Assert.IsFalse(results.AllChecksSuccessful);

            var resultAsMarkdown = results.ResultsAsMarkDown(HealthCheckNotificationVerbosity.Summary);
            Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 1' all completed successfully.") > -1);
            Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 2' completed with errors.") > -1);
            Assert.IsTrue(resultAsMarkdown.IndexOf("Checks for 'Stub check 3' completed with errors.") > -1);
        }

        [Test]
        public void HealthCheckResults_WithSummaryVerbosity_ReturnsCorrectResultDescription()
        {
            var checks = new List<HealthCheck>
            {
                new StubHealthCheck1(StatusResultType.Success, "First check was successful"),
                new StubHealthCheck2(StatusResultType.Success, "Second check was successful"),
            };
            var results = new HealthCheckResults(checks);

            var resultAsMarkdown = results.ResultsAsMarkDown(HealthCheckNotificationVerbosity.Summary);
            Assert.IsTrue(resultAsMarkdown.IndexOf("Result: 'Success'\r\n") > -1);
        }

        [Test]
        public void HealthCheckResults_WithDetailedVerbosity_ReturnsCorrectResultDescription()
        {
            var checks = new List<HealthCheck>
            {
                new StubHealthCheck1(StatusResultType.Success, "First check was successful"),
                new StubHealthCheck2(StatusResultType.Success, "Second check was successful"),
            };
            var results = new HealthCheckResults(checks);

            var resultAsMarkdown = results.ResultsAsMarkDown(HealthCheckNotificationVerbosity.Detailed);
            Assert.IsFalse(resultAsMarkdown.IndexOf("Result: 'Success'\r\n") > -1);
            Assert.IsTrue(resultAsMarkdown.IndexOf("Result: 'Success', Message: 'First check was successful'\r\n") > -1);
        }
    }
}
