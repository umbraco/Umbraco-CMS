using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
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
    private Mock<ILongRunningOperationRepository> _longRunningOperationRepositoryMock;
    private Mock<TimeProvider> _timeProviderMock;
    private Mock<ICoreScope> _scopeMock;

    [SetUp]
    public void Setup()
    {
        _scopeProviderMock = new Mock<ICoreScopeProvider>(MockBehavior.Strict);
        _longRunningOperationRepositoryMock = new Mock<ILongRunningOperationRepository>(MockBehavior.Strict);
        _timeProviderMock = new Mock<TimeProvider>(MockBehavior.Strict);
        _scopeMock = new Mock<ICoreScope>();

        _longRunningOperationService = new LongRunningOperationService(
            Options.Create(new LongRunningOperationsSettings()),
            _longRunningOperationRepositoryMock.Object,
            _scopeProviderMock.Object,
            _timeProviderMock.Object,
            Mock.Of<ILogger<LongRunningOperationService>>());
    }

    [Test]
    public async Task Run_ReturnsFailedAttempt_WhenOperationIsAlreadyRunning()
    {
        SetupScopeProviderMock();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.GetByTypeAsync("Test", It.IsAny<LongRunningOperationStatus[]>(), 0, 0))
            .Callback<string, LongRunningOperationStatus[], int, int>((_, statuses, _, _) =>
            {
                Assert.That(statuses.Length, Is.EqualTo(2));
                Assert.That(statuses, Does.Contain(LongRunningOperationStatus.Enqueued));
                Assert.That(statuses, Does.Contain(LongRunningOperationStatus.Running));
            })
            .ReturnsAsync(
                new PagedModel<LongRunningOperation>
                {
                    Total = 1,
                    Items = new List<LongRunningOperation> { new() { Id = Guid.NewGuid(), Type = "Test", Status = LongRunningOperationStatus.Running } },
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.RunAsync(
            "Test",
            _ => Task.CompletedTask,
            allowConcurrentExecution: false,
            runInBackground: true);

        _longRunningOperationRepositoryMock.VerifyAll();

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LongRunningOperationEnqueueStatus.AlreadyRunning));
    }

    [Test]
    public async Task Run_CreatesAndRunsOperation_WhenNotInBackground()
    {
        SetupScopeProviderMock();

        _timeProviderMock.Setup(repo => repo.GetUtcNow())
            .Returns(() => DateTime.UtcNow)
            .Verifiable(Times.Exactly(2));

        _longRunningOperationRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<LongRunningOperation>(), It.IsAny<DateTimeOffset>()))
            .Callback<LongRunningOperation, DateTimeOffset>((op, exp) =>
            {
                Assert.That(op.Type, Is.EqualTo("Test"));
                Assert.That(op.Id, Is.Not.Null);
                Assert.That(op.Status, Is.EqualTo(LongRunningOperationStatus.Enqueued));
            })
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        _scopeProviderMock.Setup(scopeProvider => scopeProvider.Context)
            .Returns(default(IScopeContext?))
            .Verifiable(Times.Exactly(1));

        var expectedStatuses = new List<LongRunningOperationStatus>
        {
            LongRunningOperationStatus.Enqueued,
            LongRunningOperationStatus.Running,
            LongRunningOperationStatus.Success,
        };

        _longRunningOperationRepositoryMock.Setup(repo => repo.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<LongRunningOperationStatus>(), It.IsAny<DateTimeOffset>()))
            .Callback<Guid, LongRunningOperationStatus, DateTimeOffset>((id, status, exp) =>
            {
                Assert.That(expectedStatuses, Does.Contain(status));
            })
            .Returns(Task.CompletedTask);

        var opCalls = 0;
        var result = await _longRunningOperationService.RunAsync(
            "Test",
            _ =>
            {
                opCalls++;
                return Task.CompletedTask;
            },
            allowConcurrentExecution: true,
            runInBackground: false);

        _longRunningOperationRepositoryMock.VerifyAll();

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(LongRunningOperationEnqueueStatus.Success));
        Assert.That(opCalls, Is.EqualTo(1), "Operation should have run and increased the call count, since it's not configured to run in the background.");
    }

    [Test]
    public void Run_ThrowsException_WhenAttemptingToRunOperationNotInBackgroundInsideAScope()
    {
        SetupScopeProviderMock();

        _scopeProviderMock.Setup(scopeProvider => scopeProvider.Context)
            .Returns(new ScopeContext())
            .Verifiable(Times.Exactly(1));

        var opCalls = 0;
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _longRunningOperationService.RunAsync(
            "Test",
            _ =>
            {
                opCalls++;
                return Task.CompletedTask;
            },
            allowConcurrentExecution: true,
            runInBackground: false));
        Assert.That(opCalls, Is.EqualTo(0), "The operation should not have been called.");
    }

    [Test]
    public async Task Run_CreatesAndQueuesOperation_WhenInBackground()
    {
        SetupScopeProviderMock();

        _timeProviderMock.Setup(repo => repo.GetUtcNow())
            .Returns(() => DateTime.UtcNow)
            .Verifiable(Times.Exactly(2));

        _longRunningOperationRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<LongRunningOperation>(), It.IsAny<DateTimeOffset>()))
            .Callback<LongRunningOperation, DateTimeOffset>((op, exp) =>
            {
                Assert.That(op.Type, Is.EqualTo("Test"));
                Assert.That(op.Id, Is.Not.Null);
                Assert.That(op.Status, Is.EqualTo(LongRunningOperationStatus.Enqueued));
            })
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.RunAsync(
            "Test",
            _ => Task.CompletedTask,
            allowConcurrentExecution: true,
            runInBackground: true);

        _longRunningOperationRepositoryMock.VerifyAll();

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(LongRunningOperationEnqueueStatus.Success));
    }

    [Test]
    public async Task GetStatus_ReturnsExpectedStatus_WhenOperationExists()
    {
        SetupScopeProviderMock();
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.GetStatusAsync(operationId))
            .ReturnsAsync(LongRunningOperationStatus.Running)
            .Verifiable(Times.Once);

        var status = await _longRunningOperationService.GetStatusAsync(operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.That(status.HasValue, Is.True);
        Assert.That(status.Value, Is.EqualTo(LongRunningOperationStatus.Running));
    }

    [Test]
    public async Task GetStatus_ReturnsNull_WhenOperationDoesNotExist()
    {
        SetupScopeProviderMock();
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.GetStatusAsync(operationId))
            .ReturnsAsync((LongRunningOperationStatus?)null)
            .Verifiable(Times.Once);

        var status = await _longRunningOperationService.GetStatusAsync(operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.That(status.HasValue, Is.False);
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
            .Setup(repo => repo.GetByTypeAsync(operationType, It.IsAny<LongRunningOperationStatus[]>(), 0, 100))
            .Callback<string, LongRunningOperationStatus[], int, int>((_, statuses, _, _) =>
            {
                Assert.That(statuses.Length, Is.EqualTo(2));
                Assert.That(statuses, Does.Contain(LongRunningOperationStatus.Enqueued));
                Assert.That(statuses, Does.Contain(LongRunningOperationStatus.Running));
            })
            .ReturnsAsync(
                new PagedModel<LongRunningOperation>
                {
                    Total = 2,
                    Items = operations,
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetByTypeAsync(operationType, 0, 100);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count(), Is.EqualTo(2));
        Assert.That(result.Total, Is.EqualTo(2));
        Assert.That(result.Items.All(op => op.Type == operationType), Is.True);
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
            .Setup(repo => repo.GetByTypeAsync(operationType, It.IsAny<LongRunningOperationStatus[]>(), 0, 30))
            .Callback<string, LongRunningOperationStatus[], int, int>((type, statuses, _, _) =>
            {
                Assert.That(statuses.Length, Is.EqualTo(1));
                Assert.That(statuses, Does.Contain(LongRunningOperationStatus.Failed));
            })
            .ReturnsAsync(
                new PagedModel<LongRunningOperation>
                {
                    Total = 1,
                    Items = operations,
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetByTypeAsync(operationType, 0, 30, [LongRunningOperationStatus.Failed]);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Total, Is.EqualTo(1));
        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Items.All(op => op.Type == operationType), Is.True);
    }

    [Test]
    public async Task GetResult_ReturnsExpectedResult_WhenOperationExists()
    {
        SetupScopeProviderMock();
        const string operationType = "Test";
        var operationId = Guid.NewGuid();
        const string expectedResult = "TestResult";
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.GetAsync<string>(operationId))
            .ReturnsAsync(
                new LongRunningOperation<string>
                {
                    Id = operationId,
                    Type = operationType,
                    Status = LongRunningOperationStatus.Success,
                    Result = expectedResult,
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResultAsync<string>(operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(LongRunningOperationResultStatus.Success));
        Assert.That(result.Result, Is.EqualTo(expectedResult));
    }

    [Test]
    public async Task GetResult_ReturnsFailedAttempt_WhenOperationDoesNotExist()
    {
        SetupScopeProviderMock();
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.GetAsync<string>(operationId))
            .ReturnsAsync(default(LongRunningOperation<string>))
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResultAsync<string>(operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LongRunningOperationResultStatus.OperationNotFound));
        Assert.That(result.Result, Is.Null);
    }

    [Test]
    public async Task GetResult_ReturnsFailedAttempt_WhenOperationFailed()
    {
        SetupScopeProviderMock();
        const string operationType = "Test";
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.GetAsync<string>(operationId))
            .ReturnsAsync(
                new LongRunningOperation<string>
                {
                    Id = operationId,
                    Type = operationType,
                    Status = LongRunningOperationStatus.Failed,
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResultAsync<string>(operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LongRunningOperationResultStatus.OperationFailed));
        Assert.That(result.Result, Is.Null);
    }

    [Test]
    public async Task GetResult_ReturnsFailedAttempt_WhenOperationIsRunning()
    {
        SetupScopeProviderMock();
        const string operationType = "Test";
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.GetAsync<string>(operationId))
            .ReturnsAsync(
                new LongRunningOperation<string>
                {
                    Id = operationId,
                    Type = operationType,
                    Status = LongRunningOperationStatus.Running,
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResultAsync<string>(operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LongRunningOperationResultStatus.OperationPending));
        Assert.That(result.Result, Is.Null);
    }

    [Test]
    public async Task GetResult_ReturnsFailedAttempt_WhenOperationIsEnqueued()
    {
        SetupScopeProviderMock();
        const string operationType = "Test";
        var operationId = Guid.NewGuid();
        _longRunningOperationRepositoryMock
            .Setup(repo => repo.GetAsync<string>(operationId))
            .ReturnsAsync(
                new LongRunningOperation<string>
                {
                    Id = operationId,
                    Type = operationType,
                    Status = LongRunningOperationStatus.Enqueued,
                })
            .Verifiable(Times.Once);

        var result = await _longRunningOperationService.GetResultAsync<string>(operationId);

        _longRunningOperationRepositoryMock.VerifyAll();
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(LongRunningOperationResultStatus.OperationPending));
        Assert.That(result.Result, Is.Null);
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


