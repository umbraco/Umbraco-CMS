// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

[TestFixture]
public class TaskHelperTests
{
    [Test]
    [AutoMoqData]
    public void RunBackgroundTask__Suppress_Execution_Context(
        [Frozen] ILogger<TaskHelper> logger,
        TaskHelper sut)
    {
        var local = new AsyncLocal<string> { Value = "hello" };

        string taskResult = null;

        var t = sut.ExecuteBackgroundTask(() =>
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
        [Frozen] ILogger<TaskHelper> logger,
        TaskHelper sut)
    {
        var i = 0;
        var t = sut.ExecuteBackgroundTask(() =>
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
        [Frozen] ILogger<TaskHelper> logger,
        Exception exception,
        TaskHelper sut)
    {
        var t = sut.ExecuteBackgroundTask(() => throw exception);

        Task.WaitAll(t);

        Mock.Get(logger).VerifyLogError(exception, "Exception thrown in a background thread", Times.Once());
    }
}
