using System.Data.Common;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class LongRunningOperationRepositoryTests : UmbracoIntegrationTest
{
    [Test]
    public void Get_ReturnsNull_WhenOperationDoesNotExist()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var result = repository.Get(Guid.NewGuid());
        Assert.IsNull(result);
    }

    [Test]
    public void Get_ReturnsExpectedOperation_WhenOperationExists()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var testOperation = _operations[1];
        var result = repository.Get(testOperation.Operation.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(testOperation.Operation.Id, result.Id);
        Assert.AreEqual(testOperation.Operation.Type, result.Type);
        Assert.AreEqual(testOperation.Operation.Status, result.Status);
    }

    [TestCase("Test", new LongRunningOperationStatus[] { }, 5)]
    [TestCase("Test", new[] { LongRunningOperationStatus.Enqueued }, 1)]
    [TestCase("Test", new[] { LongRunningOperationStatus.Running }, 1)]
    [TestCase("Test", new[] { LongRunningOperationStatus.Enqueued, LongRunningOperationStatus.Running }, 2)]
    [TestCase("Test", new[] { LongRunningOperationStatus.Stale }, 1)]
    [TestCase("Test", new[] { LongRunningOperationStatus.Running, LongRunningOperationStatus.Stale }, 2)]
    [TestCase("Test", new[] { LongRunningOperationStatus.Success, LongRunningOperationStatus.Stale }, 2)]
    [TestCase("AnotherTest", new LongRunningOperationStatus[] { }, 1)]
    public void GetByType_ReturnsExpectedOperations(string type, LongRunningOperationStatus[] statuses, int expectedCount)
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var result = repository.GetByType(type, statuses).ToList();

        Assert.IsNotNull(result);
        Assert.AreEqual(expectedCount, result.Count);
    }

    [Test]
    public void GetStatus_ReturnsNull_WhenOperationDoesNotExist()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var result = repository.GetStatus(Guid.NewGuid());
        Assert.IsNull(result);
    }

    [Test]
    public void GetStatus_ReturnsExpectedStatus_WhenOperationExists()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var result = repository.GetStatus(_operations[0].Operation.Id);
        Assert.AreEqual(_operations[0].Operation.Status, result);
    }

    [Test]
    public void Create_InsertsOperationIntoDatabase()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var newOperation = new LongRunningOperation
        {
            Id = Guid.NewGuid(),
            Type = "NewTest",
            Status = LongRunningOperationStatus.Enqueued,
        };
        repository.Create(newOperation, DateTimeOffset.UtcNow.AddMinutes(5));

        var result = repository.Get(newOperation.Id);
        Assert.IsNotNull(result);
        Assert.AreEqual(newOperation.Id, result.Id);
        Assert.AreEqual(newOperation.Type, result.Type);
        Assert.AreEqual(newOperation.Status, result.Status);
    }

    [Test]
    public void Create_ThrowsException_WhenOperationWithTheSameIdExists()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var newOperation = new LongRunningOperation
        {
            Id = _operations[0].Operation.Id,
            Type = "NewTest",
            Status = LongRunningOperationStatus.Enqueued,
        };
        Assert.Throws(Is.InstanceOf<DbException>(), () => repository.Create(newOperation, DateTimeOffset.UtcNow.AddMinutes(5)));
    }

    [Test]
    public void UpdateStatus_UpdatesOperationStatusInDatabase()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var testOperation = _operations[1];
        repository.UpdateStatus(testOperation.Operation.Id, LongRunningOperationStatus.Failed, DateTimeOffset.UtcNow);

        var result = repository.Get(testOperation.Operation.Id);
        Assert.IsNotNull(result);
        Assert.AreEqual(LongRunningOperationStatus.Failed, result.Status);
    }

    [Test]
    public void SetResult_UpdatesOperationResultInDatabase()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var testOperation = _operations[1];
        var opResult = new LongRunningOperationResult { Result = true };
        repository.SetResult(testOperation.Operation.Id, opResult);

        var result = repository.Get<LongRunningOperationResult>(testOperation.Operation.Id);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(opResult.Result, result.Result.Result);
    }

    [Test]
    public async Task CleanOperations_RemovesOldOperationsFromTheDatabase()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        await Task.Delay(TimeSpan.FromSeconds(10));

        var oldOperation = _operations[0];

        // Check that the operation is present before cleaning
        var result = repository.Get(oldOperation.Operation.Id);
        Assert.IsNotNull(result);

        repository.CleanOperations(DateTimeOffset.UtcNow.AddSeconds(-2));

        // Check that the expired operation is removed after cleaning
        result = repository.Get(oldOperation.Operation.Id);
        Assert.IsNull(result);
    }

    private LongRunningOperationRepository CreateRepository(IScopeProvider provider)
        => new(GetRequiredService<IJsonSerializer>(), (IScopeAccessor)provider, AppCaches.Disabled, TimeProvider.System);

    private void CreateTestData(LongRunningOperationRepository repository) =>
        _operations.ForEach(op => repository.Create(op.Operation, op.ExpiresIn));

    private readonly List<(LongRunningOperation Operation, DateTimeOffset ExpiresIn)> _operations =
    [
        (
            Operation: new LongRunningOperation { Id = Guid.NewGuid(), Type = "Test", Status = LongRunningOperationStatus.Success },
            ExpiresIn: DateTimeOffset.UtcNow.AddMinutes(5)),
        (
            Operation: new LongRunningOperation { Id = Guid.NewGuid(), Type = "Test", Status = LongRunningOperationStatus.Enqueued },
            ExpiresIn: DateTimeOffset.UtcNow.AddMinutes(5)),
        (
            Operation: new LongRunningOperation { Id = Guid.NewGuid(), Type = "Test", Status = LongRunningOperationStatus.Running },
            ExpiresIn: DateTimeOffset.UtcNow.AddMinutes(5)),
        (
            Operation: new LongRunningOperation { Id = Guid.NewGuid(), Type = "Test", Status = LongRunningOperationStatus.Running },
            ExpiresIn: DateTimeOffset.UtcNow.AddMinutes(-1)),
        (
            Operation: new LongRunningOperation { Id = Guid.NewGuid(), Type = "Test", Status = LongRunningOperationStatus.Failed },
            ExpiresIn: DateTimeOffset.UtcNow.AddMinutes(-1)),
        (
            Operation: new LongRunningOperation { Id = Guid.NewGuid(), Type = "AnotherTest", Status = LongRunningOperationStatus.Success, },
            ExpiresIn: DateTimeOffset.UtcNow.AddMinutes(5)),
    ];

    private class LongRunningOperationResult
    {
        public bool Result { get; init; }
    }
}
