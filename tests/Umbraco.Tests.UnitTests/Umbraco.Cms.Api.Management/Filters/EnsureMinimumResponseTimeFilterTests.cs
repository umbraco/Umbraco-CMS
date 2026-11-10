// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Filters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Filters;

[TestFixture]
public class EnsureMinimumResponseTimeFilterTests
{
    private static readonly TimeSpan MinimumResponseTime = TimeSpan.FromSeconds(2);

    [TestCase(500)]
    [TestCase(1234.567)]
    public async Task Delays_The_Response_Until_The_Minimum_Response_Time_Has_Elapsed(double actionMilliseconds)
    {
        var timeProvider = new FakeTimeProvider();
        var sut = new TestEnsureMinimumResponseTimeFilter(MinimumResponseTime, timeProvider);

        Task execution = sut.OnActionExecutionAsync(null!, CreateAction(timeProvider, TimeSpan.FromMilliseconds(actionMilliseconds)));

        Assert.IsFalse(execution.IsCompleted, "The response was not delayed.");

        timeProvider.Advance(MinimumResponseTime - TimeSpan.FromMilliseconds(actionMilliseconds));
        await execution.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task Does_Not_Delay_The_Response_When_The_Minimum_Response_Time_Has_Already_Elapsed()
    {
        var timeProvider = new FakeTimeProvider();
        var sut = new TestEnsureMinimumResponseTimeFilter(MinimumResponseTime, timeProvider);

        Task execution = sut.OnActionExecutionAsync(null!, CreateAction(timeProvider, MinimumResponseTime + TimeSpan.FromMilliseconds(1)));

        Assert.IsTrue(execution.IsCompleted, "The response was delayed.");
        await execution;
    }

    private static ActionExecutionDelegate CreateAction(FakeTimeProvider timeProvider, TimeSpan duration)
        => () =>
        {
            timeProvider.Advance(duration);
            return Task.FromResult<ActionExecutedContext>(null!);
        };

    private sealed class TestEnsureMinimumResponseTimeFilter : EnsureMinimumResponseTimeFilter
    {
        public TestEnsureMinimumResponseTimeFilter(TimeSpan minimumResponseTime, TimeProvider timeProvider)
            : base(minimumResponseTime, timeProvider)
        {
        }
    }
}
