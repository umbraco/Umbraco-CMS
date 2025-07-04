using System.Data.Common;
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

        var result = repository.Get("Test", Guid.NewGuid());
        Assert.IsNull(result);
    }

    [Test]
    public void Get_ReturnsNull_WhenTypeDoesNotMatch()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var testOperation = _operations[0];
        var result = repository.Get("AnotherType", testOperation.Id);
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
        var result = repository.Get(testOperation.Type, testOperation.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(testOperation.Id, result.Id);
        Assert.AreEqual(testOperation.Type, result.Type);
        Assert.AreEqual(testOperation.Status, result.Status);
    }

    [Test]
    public void GetLatest_ReturnsNull_WhenThereIsNoOperationOfThatType()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var result = repository.GetLatest("NonExisting");
        Assert.IsNull(result);
    }

    [Test]
    public void GetLatest_ReturnsExpectedOperation_WhenOperationExists()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        CreateTestData(repository);

        var expectedOperation = _operations[1];
        var result = repository.GetLatest("Test");

        Assert.IsNotNull(result);
        Assert.AreEqual(expectedOperation.Id, result.Id);
        Assert.AreEqual(expectedOperation.Type, result.Type);
        Assert.AreEqual(expectedOperation.Status, result.Status);
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

        var result = repository.Get(newOperation.Type, newOperation.Id);
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

        var testOperation = _operations[0];
        repository.UpdateStatus(testOperation.Id, LongRunningOperationStatus.Failed, TimeSpan.Zero);

        var result = repository.Get(testOperation.Type, testOperation.Id);
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

        var testOperation = _operations[0];
        var opResult = new LongRunningOperationResult { Result = true };
        repository.SetResult(testOperation.Id, opResult, TimeSpan.FromMinutes(5));

        var result = repository.GetResult<LongRunningOperationResult>(testOperation.Id);
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Result, opResult.Result);
    }

    [Test]
    public void CleanOperations_RemovesExpiredOperationsFromDatabase()
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
        var result = repository.Get(expiredOperation.Type, expiredOperation.Id);
        Assert.IsNotNull(result);

        repository.CleanOperations();

        // Check that the expired operation is removed after cleaning
        result = repository.Get(expiredOperation.Type, expiredOperation.Id);
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

