using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PropertyTypeUsageRepositoryTest : UmbracoIntegrationTestWithContent
{
    private IPropertyTypeUsageRepository Repository => GetRequiredService<IPropertyTypeUsageRepository>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    [Test]
    public void Repository_IsResolvableFromTheContainer() =>
        Assert.That(Repository, Is.InstanceOf<PropertyTypeUsageRepository>());

    [Test]
    public async Task HasSavedPropertyValuesAsync_PropertyWithSavedValues_ReturnsTrue()
    {
        using IEFCoreScope<UmbracoDbContext> scope = NewScopeProvider.CreateScope();

        var hasSavedValues = await Repository.HasSavedPropertyValuesAsync(new Guid(TextpageContentTypeKey), "title", CancellationToken.None);

        Assert.That(hasSavedValues, Is.True);
    }

    [Test]
    public async Task HasSavedPropertyValuesAsync_PropertyWithoutSavedValues_ReturnsFalse()
    {
        ContentType unusedContentType = ContentTypeBuilder.CreateSimpleContentType("unusedType", "Unused Type");
        Attempt<ContentTypeOperationStatus> created = await ContentTypeService.CreateAsync(unusedContentType, Constants.Security.SuperUserKey);
        Assert.That(created.Success, Is.True);

        using IEFCoreScope<UmbracoDbContext> scope = NewScopeProvider.CreateScope();

        var hasSavedValues = await Repository.HasSavedPropertyValuesAsync(unusedContentType.Key, "title", CancellationToken.None);

        Assert.That(hasSavedValues, Is.False);
    }

    [Test]
    public async Task HasSavedPropertyValuesAsync_UnknownPropertyAlias_ReturnsFalse()
    {
        using IEFCoreScope<UmbracoDbContext> scope = NewScopeProvider.CreateScope();

        var hasSavedValues = await Repository.HasSavedPropertyValuesAsync(new Guid(TextpageContentTypeKey), "missingProperty", CancellationToken.None);

        Assert.That(hasSavedValues, Is.False);
    }

    [Test]
    public async Task ContentTypeExistAsync_DocumentTypeKey_ReturnsTrue()
    {
        using IEFCoreScope<UmbracoDbContext> scope = NewScopeProvider.CreateScope();

        var exists = await Repository.ContentTypeExistAsync(new Guid(TextpageContentTypeKey), CancellationToken.None);

        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ContentTypeExistAsync_MediaTypeKey_ReturnsTrue()
    {
        MediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("testMediaType", "Test Media Type");
        Attempt<ContentTypeOperationStatus> created = await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        Assert.That(created.Success, Is.True);

        using IEFCoreScope<UmbracoDbContext> scope = NewScopeProvider.CreateScope();

        var exists = await Repository.ContentTypeExistAsync(mediaType.Key, CancellationToken.None);

        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ContentTypeExistAsync_MemberTypeKey_ReturnsTrue()
    {
        MemberType memberType = MemberTypeBuilder.CreateSimpleMemberType("testMemberType", "Test Member Type");
        Attempt<ContentTypeOperationStatus> created = await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        Assert.That(created.Success, Is.True);

        using IEFCoreScope<UmbracoDbContext> scope = NewScopeProvider.CreateScope();

        var exists = await Repository.ContentTypeExistAsync(memberType.Key, CancellationToken.None);

        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ContentTypeExistAsync_DataTypeKey_ReturnsFalse()
    {
        using IEFCoreScope<UmbracoDbContext> scope = NewScopeProvider.CreateScope();

        var exists = await Repository.ContentTypeExistAsync(Constants.DataTypes.Guids.TextstringGuid, CancellationToken.None);

        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task ContentTypeExistAsync_UnknownKey_ReturnsFalse()
    {
        using IEFCoreScope<UmbracoDbContext> scope = NewScopeProvider.CreateScope();

        var exists = await Repository.ContentTypeExistAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(exists, Is.False);
    }
}
