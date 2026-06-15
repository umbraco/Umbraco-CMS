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
    public async Task Get_ReturnsNull_WhenOperationDoesNotExist()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        await CreateTestData(repository);

        var result = await repository.GetAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Get_ReturnsExpectedOperation_WhenOperationExists()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        await CreateTestData(repository);

        var testOperation = _operations[1];
        var result = await repository.GetAsync(testOperation.Operation.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(testOperation.Operation.Id));
        Assert.That(result.Type, Is.EqualTo(testOperation.Operation.Type));
        Assert.That(result.Status, Is.EqualTo(testOperation.Operation.Status));
    }

    [TestCase("Test", new LongRunningOperationStatus[] { }, 0, 100, 5, 5)]
    [TestCase("Test", new[] { LongRunningOperationStatus.Enqueued }, 0, 100, 1, 1)]
    [TestCase("Test", new[] { LongRunningOperationStatus.Running }, 0, 100, 1, 1)]
    [TestCase("Test", new[] { LongRunningOperationStatus.Enqueued, LongRunningOperationStatus.Running }, 0, 100, 2, 2)]
    [TestCase("Test", new[] { LongRunningOperationStatus.Stale }, 0, 100, 1, 1)]
    [TestCase("Test", new[] { LongRunningOperationStatus.Running, LongRunningOperationStatus.Stale }, 0, 100, 2, 2)]
    [TestCase("Test", new[] { LongRunningOperationStatus.Success, LongRunningOperationStatus.Stale }, 0, 100, 2, 2)]
    [TestCase("AnotherTest", new LongRunningOperationStatus[] { }, 0, 100, 1, 1)]
    [TestCase("Test", new LongRunningOperationStatus[] { }, 0, 0, 0, 5)]
    [TestCase("Test", new LongRunningOperationStatus[] { }, 0, 1, 1, 5)]
    [TestCase("Test", new LongRunningOperationStatus[] { }, 2, 2, 2, 5)]
    [TestCase("Test", new LongRunningOperationStatus[] { }, 5, 1, 0, 5)]
    public async Task GetByType_ReturnsExpectedOperations(string type, LongRunningOperationStatus[] statuses, int skip, int take, int expectedCount, int expectedTotal)
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        await CreateTestData(repository);

        var result = await repository.GetByTypeAsync(type, statuses, skip, take);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count(), Is.EqualTo(expectedCount), "Count of returned items should match the expected count");
        Assert.That(result.Total, Is.EqualTo(expectedTotal), "Total count should match the expected total count");
    }

    [Test]
    public async Task GetStatus_ReturnsNull_WhenOperationDoesNotExist()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        await CreateTestData(repository);

        var result = await repository.GetStatusAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetStatus_ReturnsExpectedStatus_WhenOperationExists()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        await CreateTestData(repository);

        var result = await repository.GetStatusAsync(_operations[0].Operation.Id);
        Assert.That(result, Is.EqualTo(_operations[0].Operation.Status));
    }

    [Test]
    public async Task Create_InsertsOperationIntoDatabase()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        await CreateTestData(repository);

        var newOperation = new LongRunningOperation
        {
            Id = Guid.NewGuid(),
            Type = "NewTest",
            Status = LongRunningOperationStatus.Enqueued,
        };
        await repository.CreateAsync(newOperation, DateTimeOffset.UtcNow.AddMinutes(5));

        var result = await repository.GetAsync(newOperation.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(newOperation.Id));
        Assert.That(result.Type, Is.EqualTo(newOperation.Type));
        Assert.That(result.Status, Is.EqualTo(newOperation.Status));
    }

    [Test]
    public async Task Create_ThrowsException_WhenOperationWithTheSameIdExists()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        await CreateTestData(repository);

        var newOperation = new LongRunningOperation
        {
            Id = _operations[0].Operation.Id,
            Type = "NewTest",
            Status = LongRunningOperationStatus.Enqueued,
        };
        Assert.ThrowsAsync(Is.InstanceOf<DbException>(), () => repository.CreateAsync(newOperation, DateTimeOffset.UtcNow.AddMinutes(5)));
    }

    [Test]
    public async Task UpdateStatus_UpdatesOperationStatusInDatabase()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        await CreateTestData(repository);

        var testOperation = _operations[1];
        await repository.UpdateStatusAsync(testOperation.Operation.Id, LongRunningOperationStatus.Failed, DateTimeOffset.UtcNow);

        var result = await repository.GetAsync(testOperation.Operation.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(LongRunningOperationStatus.Failed));
    }

    [Test]
    public async Task SetResult_UpdatesOperationResultInDatabase()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        await CreateTestData(repository);

        var testOperation = _operations[1];
        var opResult = new LongRunningOperationResult { Result = true };
        await repository.SetResultAsync(testOperation.Operation.Id, opResult);

        var result = await repository.GetAsync<LongRunningOperationResult>(testOperation.Operation.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Result, Is.EqualTo(opResult.Result));
    }

    [Test]
    public async Task CleanOperations_RemovesOldOperationsFromTheDatabase()
    {
        var provider = ScopeProvider;
        using var scope = provider.CreateScope();
        var repository = CreateRepository(provider);
        await CreateTestData(repository);

        var oldOperation = _operations[0];

        // Check that the operation is present before cleaning
        var result = await repository.GetAsync(oldOperation.Operation.Id);
        Assert.That(result, Is.Not.Null);

        await repository.CleanOperationsAsync(DateTimeOffset.UtcNow.AddMinutes(1));

        // Check that the operation is removed after cleaning
        result = await repository.GetAsync(oldOperation.Operation.Id);
        Assert.That(result, Is.Null);
    }

    private LongRunningOperationRepository CreateRepository(IScopeProvider provider)
        => new(GetRequiredService<IJsonSerializer>(), (IScopeAccessor)provider, AppCaches.Disabled, TimeProvider.System);

    private async Task CreateTestData(LongRunningOperationRepository repository)
    {
        foreach (var op in _operations)
        {
            await repository.CreateAsync(op.Operation, op.ExpiresIn);
        }
    }

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
