using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.New.Cms.Core.Installer;
using UpgradeService = Umbraco.New.Cms.Core.Services.Installer.UpgradeService;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.New.Cms.Core.Services;

[TestFixture]
public class UpgradeServiceTests
{

    [Test]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Unknown)]
    public void RequiresUpgradeRuntimeToUpgrade(RuntimeLevel level)
    {
        var sut = CreateUpgradeService(Enumerable.Empty<IUpgradeStep>(), level);

        Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.Upgrade());
    }

    [Test]
    public async Task OnlyRunsStepsThatRequireExecution()
    {
        var steps = new[]
        {
            new TestUpgradeStep { ShouldRun = true },
            new TestUpgradeStep { ShouldRun = false },
            new TestUpgradeStep { ShouldRun = true },
        };

        var sut = CreateUpgradeService(steps);

        await sut.Upgrade();

        foreach (var step in steps)
        {
            Assert.AreEqual(step.ShouldRun, step.HasRun);
        }
    }

    [Test]
    public async Task StepsRunInCollectionOrder()
    {
        List<TestUpgradeStep> runOrder = new List<TestUpgradeStep>();

        var steps = new[]
        {
            new TestUpgradeStep { Id = 1 },
            new TestUpgradeStep { Id = 2 },
            new TestUpgradeStep { Id = 3 },
        };

        // Add an method delegate that will add the step itself, that way we can know the executed order.
        foreach (var step in steps)
        {
            step.AdditionalExecution = () => runOrder.Add(step);
        }

        var sut = CreateUpgradeService(steps);
        await sut.Upgrade();

        // The ID's are strictly not necessary, but it makes potential debugging easier.
        var expectedRunOrder = steps.Select(x => x.Id);
        var actualRunOrder = runOrder.Select(x => x.Id);
        Assert.AreEqual(expectedRunOrder, actualRunOrder);
    }

    private UpgradeService CreateUpgradeService(IEnumerable<IUpgradeStep> steps, RuntimeLevel runtimeLevel = RuntimeLevel.Upgrade)
    {
        var logger = Mock.Of<ILogger<UpgradeService>>();
        var stepCollection = new UpgradeStepCollection(() => steps);
        var runtimeStateMock = new Mock<IRuntimeState>();
        runtimeStateMock.Setup(x => x.Level).Returns(runtimeLevel);

        return new UpgradeService(stepCollection, runtimeStateMock.Object, logger);
    }

    private class TestUpgradeStep : IUpgradeStep
    {
        public bool HasRun;

        public bool ShouldRun = true;

        public int Id;

        public Action AdditionalExecution;

        public Task ExecuteAsync()
        {
            HasRun = true;

            AdditionalExecution?.Invoke();
            return Task.CompletedTask;
        }

        public Task<bool> RequiresExecutionAsync() => Task.FromResult(ShouldRun);
    }
}
