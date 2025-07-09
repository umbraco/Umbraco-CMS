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
            .Setup(repo => repo.GetByType("Test", It.IsAny<LongRunningOperationStatus[]>()))
            .Callback<string, LongRunningOperationStatus[]>((type, statuses) =>
            {
                Assert.AreEqual(2, statuses.Length);
                Assert.Contains(LongRunningOperationStatus.Enqueued, statuses);
                Assert.Contains(LongRunningOperationStatus.Running, statuses);
            })
            .Returns(new List<LongRunningOperation> { new() { Id = Guid.NewGuid(), Type = "Test", Status = LongRunningOperationStatus.Running } })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.Run(
            "Test",
            _ => Task.CompletedTask,
            allowConcurrentExecution: false,
            runInBackground: false);

        _longRunningOperationRepositoryMock.VerifyAll();

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
        _longRunningOperationRepositoryMock.Setup(repo => repo.UpdateStatus(It.IsAny<Guid>(), It.IsAny<LongRunningOperationStatus>(), It.IsAny<TimeSpan>()))
            .Callback<Guid, LongRunningOperationStatus, TimeSpan>((id, status, exp) =>
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
            allowConcurrentExecution: true,
            runInBackground: false,
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
            allowConcurrentExecution: true,
            runInBackground: true,
            expires);

        _longRunningOperationRepositoryMock.VerifyAll();
        _backgroundTaskQueueMock.VerifyAll();

        Assert.IsTrue(result.Success);
        Assert.AreEqual(LongRunningOperationEnqueueStatus.Success, result.Status);
        Assert.AreEqual(0, opCalls, "Operation should not be called immediately when run in background, it will be executed later by the background task queue.");
    }

    [Test]
    public async Task GetStatus_ReturnsExpectedStatus_WhenOperationExists()
    {
        SetupScopeProviderMock();
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.GetStatus(operationId))
            .Returns(LongRunningOperationStatus.Running)
            .Verifiable(Times.Once);

        var status = await _longRunningOperationService.GetStatus(operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.IsTrue(status.HasValue);
        Assert.AreEqual(LongRunningOperationStatus.Running, status.Value);
    }

    [Test]
    public async Task GetStatus_ReturnsNull_WhenOperationDoesNotExist()
    {
        SetupScopeProviderMock();
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.GetStatus(operationId))
            .Returns((LongRunningOperationStatus?)null)
            .Verifiable(Times.Once);

        var status = await _longRunningOperationService.GetStatus(operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.IsFalse(status.HasValue);
    }

    [Test]
    public async Task GetByType_ReturnsExpectedOperations_WhenOperationsExist()
    {
        SetupScopeProviderMock();
        const string operationType = "Test";
        var operations = new List<LongRunningOperation>
        {
            new() { Id = Guid.NewGuid(), Type = operationType, Status = LongRunningOperationStatus.Running },
            new() { Id = Guid.NewGuid(), Type = operationType, Status = LongRunningOperationStatus.Enqueued },
        };
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.GetByType(operationType, It.IsAny<LongRunningOperationStatus[]>()))
            .Callback<string, LongRunningOperationStatus[]>((type, statuses) =>
            {
                Assert.AreEqual(2, statuses.Length);
                Assert.Contains(LongRunningOperationStatus.Enqueued, statuses);
                Assert.Contains(LongRunningOperationStatus.Running, statuses);
            })
            .Returns(operations)
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetByType(operationType);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count());
        Assert.IsTrue(result.All(op => op.Type == operationType));
    }

    [Test]
    public async Task GetByType_ReturnsExpectedOperations_WhenOperationsExistWithProvidedStatuses()
    {
        SetupScopeProviderMock();
        const string operationType = "Test";
        var operations = new List<LongRunningOperation>
        {
            new() { Id = Guid.NewGuid(), Type = operationType, Status = LongRunningOperationStatus.Failed },
        };
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.GetByType(operationType, It.IsAny<LongRunningOperationStatus[]>()))
            .Callback<string, LongRunningOperationStatus[]>((type, statuses) =>
            {
                Assert.AreEqual(1, statuses.Length);
                Assert.Contains(LongRunningOperationStatus.Failed, statuses);
            })
            .Returns(operations)
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetByType(operationType, [LongRunningOperationStatus.Failed]);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.IsTrue(result.All(op => op.Type == operationType));
    }

    [Test]
    public async Task GetResult_ReturnsExpectedResult_WhenOperationExists()
    {
        SetupScopeProviderMock();
        const string operationType = "Test";
        var operationId = Guid.NewGuid();
        const string expectedResult = "TestResult";
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.Get<string>(operationId))
            .Returns(
                new LongRunningOperation<string>
                {
                    Id = operationId,
                    Type = operationType,
                    Status = LongRunningOperationStatus.Success,
                    Result = expectedResult,
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResult<string>(operationId);

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
            .Setup(repo => repo.Get<string>(operationId))
            .Returns(default(LongRunningOperation<string>))
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResult<string>(operationId);

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
            .Setup(repo => repo.Get<string>(operationId))
            .Returns(
                new LongRunningOperation<string>
                {
                    Id = operationId,
                    Type = operationType,
                    Status = LongRunningOperationStatus.Failed,
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResult<string>(operationId);

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
            .Setup(repo => repo.Get<string>(operationId))
            .Returns(
                new LongRunningOperation<string>
                {
                    Id = operationId,
                    Type = operationType,
                    Status = LongRunningOperationStatus.Running,
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResult<string>(operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Status, LongRunningOperationResultStatus.OperationPending);
        Assert.IsNull(result.Result);
    }

    [Test]
    public async Task GetResult_ReturnsFailedAttempt_WhenOperationIsEnqueued()
    {
        SetupScopeProviderMock();
        const string operationType = "Test";
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.Get<string>(operationId))
            .Returns(
                new LongRunningOperation<string>
                {
                    Id = operationId,
                    Type = operationType,
                    Status = LongRunningOperationStatus.Enqueued,
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResult<string>(operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Status, LongRunningOperationResultStatus.OperationPending);
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

