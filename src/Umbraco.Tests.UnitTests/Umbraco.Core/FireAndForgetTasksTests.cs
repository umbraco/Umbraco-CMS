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
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core
{
    [TestFixture]
    public class FireAndForgetTasksTests
    {
        [Test]
        [AutoMoqData]
        public void RunBackgroundTask__Suppress_Execution_Context(
            [Frozen] ILogger<FireAndForgetTasks> logger,
            FireAndForgetTasks sut)
        {
            var local = new AsyncLocal<string>
            {
                Value = "hello"
            };

            string taskResult = null;
            
            Task t = sut.RunBackgroundTask(() =>
            {
                // FireAndForgetTasks ensure that flow is suppressed therefore this value will be null
                taskResult = local.Value;
                return Task.CompletedTask;
            });

            Task.WaitAll(t);

            Assert.IsNull(taskResult);
        }

        [Test]
        [AutoMoqData]
        public void RunBackgroundTask__Must_Run_Func(
            [Frozen] ILogger<FireAndForgetTasks> logger,
            FireAndForgetTasks sut)
        {
            var i = 0;
            Task t = sut.RunBackgroundTask(() =>
            {
                Interlocked.Increment(ref i);
                return Task.CompletedTask;
            });

            Task.WaitAll(t);

            Assert.AreEqual(1, i);
        }

        [Test]
        [AutoMoqData]
        public void RunBackgroundTask__Log_Error_When_Exception_Happen_In_Background_Task(
            [Frozen] ILogger<FireAndForgetTasks> logger,
            Exception exception,
            FireAndForgetTasks sut)
        {
            Task t = sut.RunBackgroundTask(() => throw exception);

            Task.WaitAll(t);

            Mock.Get(logger).VerifyLogError(exception, "Exception thrown in a background thread", Times.Once());
        }
    }
}
