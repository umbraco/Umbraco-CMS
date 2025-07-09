using System.Data.Common;
using Bogus;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
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
        var result = repository.Get(testOperation.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(testOperation.Id, result.Id);
        Assert.AreEqual(testOperation.Type, result.Type);
        Assert.AreEqual(testOperation.Status, result.Status);
    }

    [Test]
    public void GetByType_ReturnsOperationsOfType()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var testOperation = _operations[1];
        var result = repository.GetByType(testOperation.Type, []).ToList();

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(o => o.Type == testOperation.Type));
    }

    [Test]
    public void GetByType_ReturnsFilteredOperationsOfType_WhenFilterIsProvided()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var testOperation = _operations[1];
        var result = repository.GetByType(testOperation.Type, [LongRunningOperationStatus.Running]).ToList();

        var expectedOperations = _operations
            .Where(o => o.Type == testOperation.Type && o.Status == LongRunningOperationStatus.Running)
            .ToList();
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedOperations.Count, result.Count);
        Assert.IsTrue(expectedOperations.All(o1 => result.Any(o2 => o1.Id == o2.Id)));
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

        var result = repository.GetStatus(_operations[0].Id);
        Assert.AreEqual(_operations[0].Status, result);
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
        repository.Create(newOperation, TimeSpan.FromMinutes(5));

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
            Id = _operations[0].Id,
            Type = "NewTest",
            Status = LongRunningOperationStatus.Enqueued,
        };
        Assert.Throws(Is.InstanceOf<DbException>(), () => repository.Create(newOperation, TimeSpan.FromMinutes(5)));
    }

    [Test]
    public void UpdateStatus_UpdatesOperationStatusInDatabase()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var testOperation = _operations[1];
        repository.UpdateStatus(testOperation.Id, LongRunningOperationStatus.Failed, TimeSpan.Zero);

        var result = repository.Get(testOperation.Id);
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
        repository.SetResult(testOperation.Id, opResult);

        var result = repository.Get<LongRunningOperationResult>(testOperation.Id);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(opResult.Result, result.Result.Result);
    }

    [Test]
    public async Task CleanOperations_MarksOperationsAsFailedIfExpired()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        // Create an expired operation
        var expiredOperation = new LongRunningOperation
        {
            Id = Guid.NewGuid(),
            Type = "ExpiredTest",
            Status = LongRunningOperationStatus.Running,
        };
        repository.Create(expiredOperation, TimeSpan.FromMinutes(-5));

        // Check that the expired operation is present before cleaning
        var result = repository.Get(expiredOperation.Id);
        Assert.IsNotNull(result);

        repository.CleanOperations(TimeSpan.Zero);

        // Check that the expired operation is removed after cleaning
        result = repository.Get(expiredOperation.Id);
        Assert.IsNotNull(result);
        Assert.AreEqual(LongRunningOperationStatus.Failed, result.Status);
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
        var result = repository.Get(oldOperation.Id);
        Assert.IsNotNull(result);

        repository.CleanOperations(TimeSpan.FromSeconds(2));

        // Check that the expired operation is removed after cleaning
        result = repository.Get(oldOperation.Id);
        Assert.IsNull(result);
    }

    private LongRunningOperationRepository CreateRepository(IScopeProvider provider)
        => new(GetRequiredService<IJsonSerializer>(), (IScopeAccessor)provider, AppCaches.Disabled);

    private void CreateTestData(ILongRunningOperationRepository repository) =>
        _operations.ForEach(op => repository.Create(op, TimeSpan.FromMinutes(5)));

    private readonly List<LongRunningOperation> _operations =
    [
        new() { Id = Guid.NewGuid(), Type = "Test", Status = LongRunningOperationStatus.Success },
        new() { Id = Guid.NewGuid(), Type = "Test", Status = LongRunningOperationStatus.Running },
        new() { Id = Guid.NewGuid(), Type = "AnotherTest", Status = LongRunningOperationStatus.Success, }
    ];

    private class LongRunningOperationResult
    {
        public bool Result { get; init; }
    }
}

