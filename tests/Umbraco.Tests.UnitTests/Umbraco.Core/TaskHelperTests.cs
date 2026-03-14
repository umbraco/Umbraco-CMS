// Copyright (c) Umbraco.
// See LICENSE for more details.

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

/// <summary>
/// Contains unit tests for methods in the <see cref="TaskHelper"/> class.
/// </summary>
[TestFixture]
public class TaskHelperTests
{
    /// <summary>
    /// Verifies that when running a background task using <see cref="TaskHelper.ExecuteBackgroundTask"/>,
    /// the execution context flow is suppressed, so that <see cref="AsyncLocal{T}"/> values are not flowed into the background task.
    /// </summary>
    /// <param name="logger">The logger instance for <see cref="TaskHelper"/>.</param>
    /// <param name="sut">The <see cref="TaskHelper"/> instance under test.</param>
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

    /// <summary>
    /// Verifies that <see cref="TaskHelper.ExecuteBackgroundTask(Func{Task})"/> executes the provided function exactly once when run as a background task.
    /// </summary>
    /// <param name="logger">A logger instance for <see cref="TaskHelper"/>, provided by the test framework.</param>
    /// <param name="sut">The <see cref="TaskHelper"/> instance under test.</param>
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

    /// <summary>
    /// Verifies that when an exception is thrown in a background task, the error is logged using the provided logger.
    /// </summary>
    /// <param name="logger">The logger instance used to verify that the error is logged.</param>
    /// <param name="exception">The exception to be thrown by the background task.</param>
    /// <param name="sut">The <see cref="TaskHelper"/> instance used to execute the background task.</param>
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
