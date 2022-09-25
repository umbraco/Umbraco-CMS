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
using Umbraco.New.Cms.Core.Models.Installer;
using Umbraco.New.Cms.Core.Services.Installer;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.New.Cms.Core.Services;

[TestFixture]
public class InstallServiceTests
{
    [Test]
    public void RequiresInstallRuntimeToInstall()
    {
        var runtimeStateMock = new Mock<IRuntimeState>();
        runtimeStateMock.Setup(x => x.Level).Returns(RuntimeLevel.Run);
        var stepCollection = new NewInstallStepCollection(Enumerable.Empty<IInstallStep>);

        var sut = new InstallService(Mock.Of<ILogger<InstallService>>(), stepCollection, runtimeStateMock.Object);

        Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.Install(new InstallData()));
    }

    [Test]
    public async Task OnlyRunsStepsThatRequireExecution()
    {
        var steps = new[]
        {
            new TestInstallStep { ShouldRun = true },
            new TestInstallStep { ShouldRun = false },
            new TestInstallStep { ShouldRun = true },
        };

        var sut = CreateInstallService(steps);
        await sut.Install(new InstallData());

        foreach (var step in steps)
        {
            Assert.AreEqual(step.ShouldRun, step.HasRun);
        }
    }

    [Test]
    public async Task StepsRunInCollectionOrder()
    {
        List<TestInstallStep> runOrder = new List<TestInstallStep>();

        var steps = new[]
        {
            new TestInstallStep { Id = 1 },
            new TestInstallStep { Id = 2 },
            new TestInstallStep { Id = 3 },
        };

        // Add an method delegate that will add the step itself, that way we can know the executed order.
        foreach (var step in steps)
        {
            step.AdditionalExecution = _ =>
            {
                runOrder.Add(step);
                return Task.CompletedTask;
            };
        }

        var sut = CreateInstallService(steps);
        await sut.Install(new InstallData());

        // The ID's are strictly not necessary, but it makes potential debugging easier.
        var expectedRunOrder = steps.Select(x => x.Id);
        var actualRunOrder = runOrder.Select(x => x.Id);
        Assert.AreEqual(expectedRunOrder, actualRunOrder);
    }

    private InstallService CreateInstallService(IEnumerable<IInstallStep> steps)
    {
        var logger = Mock.Of<ILogger<InstallService>>();
        var stepCollection = new NewInstallStepCollection(() => steps);
        var runtimeStateMock = new Mock<IRuntimeState>();
        runtimeStateMock.Setup(x => x.Level).Returns(RuntimeLevel.Install);

        return new InstallService(logger, stepCollection, runtimeStateMock.Object);
    }

    private class TestInstallStep : IInstallStep
    {
        public bool HasRun;

        public bool ShouldRun = true;

        public int Id;

        public Func<InstallData, Task> AdditionalExecution;

        public Task ExecuteAsync(InstallData model)
        {
            HasRun = true;

            AdditionalExecution?.Invoke(model);
            return Task.CompletedTask;
        }

        public Task<bool> RequiresExecutionAsync(InstallData model) => Task.FromResult(ShouldRun);
    }
}
