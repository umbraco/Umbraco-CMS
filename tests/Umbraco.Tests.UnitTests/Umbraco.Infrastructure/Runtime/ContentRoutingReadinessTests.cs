// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Runtime;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Runtime;

[TestFixture]
public class ContentRoutingReadinessTests
{
    [Test]
    public void IsReady_IsFalse_ByDefault()
    {
        ContentRoutingReadiness readiness = new();

        Assert.That(readiness.IsReady, Is.False);
    }

    [Test]
    public void MarkReady_SetsIsReady()
    {
        ContentRoutingReadiness readiness = new();

        readiness.MarkReady();

        Assert.That(readiness.IsReady, Is.True);
    }

    [TestCase(RuntimeLevel.Run, false, true)]
    [TestCase(RuntimeLevel.Run, true, false)]
    [TestCase(RuntimeLevel.Upgrading, false, false)]
    [TestCase(RuntimeLevel.Upgrade, false, false)]
    [TestCase(RuntimeLevel.Install, false, false)]
    [TestCase(RuntimeLevel.BootFailed, false, false)]
    public void IsInInitializationWindow_OnlyTrue_WhenRunAndNotReady(RuntimeLevel level, bool isReady, bool expected)
    {
        var runtimeState = Mock.Of<IRuntimeState>(x => x.Level == level);
        var readiness = Mock.Of<IContentRoutingReadiness>(x => x.IsReady == isReady);

        Assert.That(readiness.IsInInitializationWindow(runtimeState), Is.EqualTo(expected));
    }
}
