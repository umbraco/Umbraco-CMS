// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Core;
using Umbraco.Tests.Common.TestHelpers;
using Umbraco.Tests.UnitTests.AutoFixture;

namespace Umbraco.Tests.UnitTests.Umbraco.Core
{
    [TestFixture]
    public class TaskHelperTests
    {
        [Test]
        [AutoMoqData]
        public void RunBackgroundTask__must_run_func([Frozen] ILogger<TaskHelper> logger, TaskHelper sut)
        {
            var i = 0;
            sut.RunBackgroundTask(() =>
            {
                Interlocked.Increment(ref i);
                return Task.CompletedTask;
            });

            Thread.Sleep(5); // Wait for background task to execute

            Assert.AreEqual(1, i);
        }

        [Test]
        [AutoMoqData]
        public void RunBackgroundTask__Log_error_when_exception_happen_in_background_task([Frozen] ILogger<TaskHelper> logger, Exception exception, TaskHelper sut)
        {
            sut.RunBackgroundTask(() => throw exception);

            Thread.Sleep(5); // Wait for background task to execute

            Mock.Get(logger).VerifyLogError(exception, "Exception thrown in a background thread", Times.Once());
        }
    }
}
