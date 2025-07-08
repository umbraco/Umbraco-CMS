using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class LongRunningOperationServiceTests
{
    private ILongRunningOperationService _longRunningOperationService;
    private Mock<ICoreScopeProvider> _scopeProviderMock;
    private Mock<IBackgroundTaskQueue> _backgroundTaskQueueMock;
    private Mock<ILongRunningOperationRepository> _longRunningOperationRepositoryMock;
    private Mock<ICoreScope> _scopeMock;

    [SetUp]
    public void Setup()
    {
        _scopeProviderMock = new Mock<ICoreScopeProvider>(MockBehavior.Strict);
        _backgroundTaskQueueMock = new Mock<IBackgroundTaskQueue>(MockBehavior.Strict);
        _longRunningOperationRepositoryMock = new Mock<ILongRunningOperationRepository>(MockBehavior.Strict);
        _scopeMock = new Mock<ICoreScope>();

        _longRunningOperationService = new LongRunningOperationService(
            _backgroundTaskQueueMock.Object,
            _longRunningOperationRepositoryMock.Object,
            _scopeProviderMock.Object,
            Mock.Of<ILogger<LongRunningOperationService>>());
    }

    [Test]
    public async Task Run_ReturnsFailedAttempt_WhenOperationIsAlreadyRunning()
    {
        SetupScopeProviderMock();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.IsEnqueuedOrRunning("Test"))
            .Returns(true)
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.Run(
            "Test",
            _ => Task.CompletedTask,
            runInBackground: false,
            allowMultipleRunsOfType: false);

        _longRunningOperationRepositoryMock.VerifyAll();

        Assert.IsFalse(result.Success);
        Assert.AreEqual(LongRunningOperationEnqueueStatus.AlreadyRunning, result.Status);
    }

    [Test]
    public async Task Run_ReturnsFailedAttempt_WhenOperationIsAlreadyRunning_BetweenChecks()
    {
        SetupScopeProviderMock();
        _longRunningOperationRepositoryMock
            .SetupSequence(repo => repo.IsEnqueuedOrRunning("Test"))
            .Returns(false)
            .Returns(true);

        var result = await _longRunningOperationService.Run(
            "Test",
            _ => Task.CompletedTask,
            runInBackground: false,
            allowMultipleRunsOfType: false);

        _longRunningOperationRepositoryMock
            .Verify(repo => repo.IsEnqueuedOrRunning("Test"), Times.Exactly(2));
        _scopeMock.Verify(scope => scope.WriteLock(Constants.Locks.LongRunningOperations), Times.Once);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(LongRunningOperationEnqueueStatus.AlreadyRunning, result.Status);
    }

    [Test]
    public async Task Run_CreatesAndRunsOperation_WhenNotInBackground()
    {
        var expires = TimeSpan.FromMinutes(10);
        SetupScopeProviderMock();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.Create(It.IsAny<LongRunningOperation>(), It.IsAny<TimeSpan>()))
            .Callback<LongRunningOperation, TimeSpan>((op, exp) =>
            {
                Assert.AreEqual("Test", op.Type);
                Assert.IsNotNull(op.Id);
                Assert.AreEqual(LongRunningOperationStatus.Enqueued, op.Status);
                Assert.AreEqual(expires, exp);
            })
            .Verifiable(Times.Once);

        var updateStatusArgsQueue = new Queue<LongRunningOperationStatus>();
        updateStatusArgsQueue.Enqueue(LongRunningOperationStatus.Running);
        updateStatusArgsQueue.Enqueue(LongRunningOperationStatus.Success);
        _longRunningOperationRepositoryMock.Setup(repo => repo.UpdateStatus(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<LongRunningOperationStatus>(), It.IsAny<TimeSpan>()))
            .Callback<string, Guid, LongRunningOperationStatus, TimeSpan>((type, id, status, exp) =>
            {
                Assert.AreEqual(updateStatusArgsQueue.Dequeue(), status);
                Assert.AreEqual(expires, exp);
            })
            .Verifiable(Times.Exactly(2));

        var opCalls = 0;
        var result = await _longRunningOperationService.Run(
            "Test",
            _ =>
            {
                opCalls++;
                return Task.CompletedTask;
            },
            runInBackground: false,
            allowMultipleRunsOfType: true,
            expires);

        _longRunningOperationRepositoryMock.VerifyAll();

        Assert.IsTrue(result.Success);
        Assert.AreEqual(LongRunningOperationEnqueueStatus.Success, result.Status);
        Assert.AreEqual(1, opCalls, "Operation should have run and increased the call count, since it's not configured to run in the background.");
    }

    [Test]
    public async Task Run_CreatesAndQueuesOperation_WhenInBackground()
    {
        var expires = TimeSpan.FromMinutes(10);
        SetupScopeProviderMock();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.Create(It.IsAny<LongRunningOperation>(), It.IsAny<TimeSpan>()))
            .Callback<LongRunningOperation, TimeSpan>((op, exp) =>
            {
                Assert.AreEqual("Test", op.Type);
                Assert.IsNotNull(op.Id);
                Assert.AreEqual(LongRunningOperationStatus.Enqueued, op.Status);
                Assert.AreEqual(expires, exp);
            })
            .Verifiable(Times.Once);

        _backgroundTaskQueueMock
            .Setup(b => b.QueueBackgroundWorkItem(It.IsAny<Func<CancellationToken, Task>>()))
            .Verifiable(Times.Once);

        var opCalls = 0;
        var result = await _longRunningOperationService.Run(
            "Test",
            _ =>
            {
                opCalls++;
                return Task.CompletedTask;
            },
            runInBackground: true,
            allowMultipleRunsOfType: true,
            expires);

        _longRunningOperationRepositoryMock.VerifyAll();
        _backgroundTaskQueueMock.VerifyAll();

        Assert.IsTrue(result.Success);
        Assert.AreEqual(LongRunningOperationEnqueueStatus.Success, result.Status);
        Assert.AreEqual(0, opCalls, "Operation should not be called immediately when run in background, it will be executed later by the background task queue.");
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task IsRunning_ReturnsExpectedResult(bool enqueuedOrRunning, bool expectedResult)
    {
        SetupScopeProviderMock();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.IsEnqueuedOrRunning("Test"))
            .Returns(enqueuedOrRunning)
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.IsRunning("Test");

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.AreEqual(expectedResult, result);
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task IsRunningWithId_ReturnsExpectedResult(bool enqueuedOrRunning, bool expectedResult)
    {
        SetupScopeProviderMock();
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.IsEnqueuedOrRunning("Test", operationId))
            .Returns(enqueuedOrRunning)
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.IsRunning("Test", operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.AreEqual(expectedResult, result);
    }

    [Test]
    public async Task GetResult_ReturnsExpectedResult_WhenOperationExists()
    {
        SetupScopeProviderMock();
        const string operationType = "Test";
        var operationId = Guid.NewGuid();
        const string expectedResult = "TestResult";
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.Get(operationType, operationId))
            .Returns(
                new LongRunningOperation
                {
                    Id = operationId,
                    Type = operationType,
                    Status = LongRunningOperationStatus.Success,
                })
            .Verifiable(Times.Once);
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.GetResult<string>(operationType, operationId))
            .Returns(expectedResult)
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResult<string>(operationType, operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.IsTrue(result.Success);
        Assert.AreEqual(LongRunningOperationResultStatus.Success, result.Status);
        Assert.AreEqual(expectedResult, result.Result);
    }

    [Test]
    public async Task GetResult_ReturnsFailedAttempt_WhenOperationDoesNotExist()
    {
        SetupScopeProviderMock();
        const string operationType = "Test";
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.Get(operationType, operationId))
            .Returns(default(LongRunningOperation))
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResult<string>(operationType, operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Status, LongRunningOperationResultStatus.OperationNotFound);
        Assert.IsNull(result.Result);
    }

    [Test]
    public async Task GetResult_ReturnsFailedAttempt_WhenOperationFailed()
    {
        SetupScopeProviderMock();
        const string operationType = "Test";
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.Get(operationType, operationId))
            .Returns(
                new LongRunningOperation
                {
                    Id = operationId,
                    Type = operationType,
                    Status = LongRunningOperationStatus.Failed,
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResult<string>(operationType, operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Status, LongRunningOperationResultStatus.OperationFailed);
        Assert.IsNull(result.Result);
    }

    [Test]
    public async Task GetResult_ReturnsFailedAttempt_WhenOperationIsRunning()
    {
        SetupScopeProviderMock();
        const string operationType = "Test";
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.Get(operationType, operationId))
            .Returns(
                new LongRunningOperation
                {
                    Id = operationId,
                    Type = operationType,
                    Status = LongRunningOperationStatus.Running,
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResult<string>(operationType, operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Status, LongRunningOperationResultStatus.OperationStillRunning);
        Assert.IsNull(result.Result);
    }

    [Test]
    public async Task GetResult_ReturnsFailedAttempt_WhenOperationIsEnqueued()
    {
        SetupScopeProviderMock();
        const string operationType = "Test";
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.Get(operationType, operationId))
            .Returns(
                new LongRunningOperation
                {
                    Id = operationId,
                    Type = operationType,
                    Status = LongRunningOperationStatus.Enqueued,
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResult<string>(operationType, operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Status, LongRunningOperationResultStatus.OperationStillRunning);
        Assert.IsNull(result.Result);
    }

    private void SetupScopeProviderMock() =>
        _scopeProviderMock
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(_scopeMock.Object);
}
