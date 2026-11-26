using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DataTypeUsageRepositoryTest : UmbracoIntegrationTestWithContent
{
    private IDataTypeUsageRepository Repository => GetRequiredService<IDataTypeUsageRepository>();

    [Test]
    public void Repository_IsResolvableFromTheContainer() =>
        Assert.That(Repository, Is.InstanceOf<DataTypeUsageRepository>());

    [Test]
    public async Task HasSavedValuesAsync_DataTypeWithSavedValues_ReturnsTrue()
    {
        using IEFCoreScope<UmbracoDbContext> scope = NewScopeProvider.CreateScope();

        var hasSavedValues = await Repository.HasSavedValuesAsync(Constants.DataTypes.Guids.TextstringGuid, CancellationToken.None);

        Assert.That(hasSavedValues, Is.True);
    }

    [Test]
    public async Task HasSavedValuesAsync_DataTypeWithoutSavedValues_ReturnsFalse()
    {
        using IEFCoreScope<UmbracoDbContext> scope = NewScopeProvider.CreateScope();

        var hasSavedValues = await Repository.HasSavedValuesAsync(Constants.DataTypes.Guids.LabelStringGuid, CancellationToken.None);

        Assert.That(hasSavedValues, Is.False);
    }

    [Test]
    public async Task HasSavedValuesAsync_UnknownDataTypeKey_ReturnsFalse()
    {
        using IEFCoreScope<UmbracoDbContext> scope = NewScopeProvider.CreateScope();

        var hasSavedValues = await Repository.HasSavedValuesAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(hasSavedValues, Is.False);
    }
}
