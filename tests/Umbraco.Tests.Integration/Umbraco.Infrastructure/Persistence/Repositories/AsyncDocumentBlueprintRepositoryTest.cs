using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AsyncDocumentBlueprintRepositoryTest : UmbracoIntegrationTest
{
    private ContentType _contentType = null!;
    private Content _textpage = null!;

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    [SetUp]
    public async Task SetUpData()
    {
        _contentType = ContentTypeBuilder.CreateSimpleContentType("umbBlueprintTextpage", "Blueprint Textpage");
        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);

        _textpage = ContentBuilder.CreateSimpleContent(_contentType);
        await ContentService.SaveAsync(_textpage, -1, null, CancellationToken.None);
    }

    private AsyncDocumentBlueprintRepository CreateRepository() => new(
        GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(),
        AppCaches.Disabled,
        LoggerFactory,
        GetRequiredService<ILanguageRepository>(),
        GetRequiredService<IRelationRepository>(),
        GetRequiredService<IRelationTypeRepository>(),
        GetRequiredService<PropertyEditorCollection>(),
        GetRequiredService<DataValueReferenceFactoryCollection>(),
        GetRequiredService<IDataTypeService>(),
        Mock.Of<IEventAggregator>(),
        Mock.Of<IRepositoryCacheVersionService>(),
        Mock.Of<ICacheSyncService>(),
        GetRequiredService<IContentTypeRepository>(),
        GetRequiredService<ITemplateRepository>(),
        GetRequiredService<IIdKeyMap>(),
        GetRequiredService<ITagRepository>(),
        GetRequiredService<IJsonSerializer>(),
        new Lazy<IUserGroupService>(GetRequiredService<IUserGroupService>),
        GetRequiredService<IShortStringHelper>());

    private AsyncDocumentRepository CreateDocumentRepository() => new(
        GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(),
        AppCaches.Disabled,
        LoggerFactory,
        GetRequiredService<ILanguageRepository>(),
        GetRequiredService<IRelationRepository>(),
        GetRequiredService<IRelationTypeRepository>(),
        GetRequiredService<PropertyEditorCollection>(),
        GetRequiredService<DataValueReferenceFactoryCollection>(),
        GetRequiredService<IDataTypeService>(),
        Mock.Of<IEventAggregator>(),
        Mock.Of<IRepositoryCacheVersionService>(),
        Mock.Of<ICacheSyncService>(),
        GetRequiredService<IContentTypeRepository>(),
        GetRequiredService<ITemplateRepository>(),
        GetRequiredService<IIdKeyMap>(),
        GetRequiredService<ITagRepository>(),
        GetRequiredService<IJsonSerializer>(),
        new Lazy<IUserGroupService>(GetRequiredService<IUserGroupService>),
        GetRequiredService<IShortStringHelper>());

    [Test]
    public async Task PersistNewItemAsync_DuplicateSiblingName_DoesNotAppendNumericSuffix()
    {
        var blueprint1 = ContentBuilder.CreateSimpleContent(_contentType, "Duplicate Blueprint Name", _textpage.Id);
        var blueprint2 = ContentBuilder.CreateSimpleContent(_contentType, "Duplicate Blueprint Name", _textpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(blueprint1, CancellationToken.None);
        await repository.SaveAsync(blueprint2, CancellationToken.None);
        scope.Complete();

        Assert.That(blueprint2.Name, Is.EqualTo("Duplicate Blueprint Name"),
            "blueprints allow duplicate names — unlike regular documents, no numeric suffix should be appended");
    }

    [Test]
    public async Task PersistNewItemAsync_PersistsDocumentBlueprintNodeObjectType()
    {
        var blueprint = ContentBuilder.CreateSimpleContent(_contentType, "Blueprint Page", _textpage.Id);

        var scopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(blueprint, CancellationToken.None);

        Guid? nodeObjectType = await scopeAccessor.AmbientScope!.ExecuteWithContextAsync(db =>
            db.Nodes.Where(n => n.NodeId == blueprint.Id).Select(n => n.NodeObjectType).SingleAsync());
        scope.Complete();

        Assert.That(nodeObjectType, Is.EqualTo(Constants.ObjectTypes.DocumentBlueprint));
    }

    [Test]
    public async Task GetChildrenAsync_OnPlainDocumentRepository_DoesNotReturnBlueprints()
    {
        var regularChild = ContentBuilder.CreateSimpleContent(_contentType, "Regular Child", _textpage.Id);
        var blueprint = ContentBuilder.CreateSimpleContent(_contentType, "Blueprint Child", _textpage.Id);

        using var scope = NewScopeProvider.CreateScope();
        var blueprintRepository = CreateRepository();
        var documentRepository = CreateDocumentRepository();

        await documentRepository.SaveAsync(regularChild, CancellationToken.None);
        await blueprintRepository.SaveAsync(blueprint, CancellationToken.None);

        PagedModel<IContent> children = await documentRepository.GetChildrenAsync(
            _textpage.Key, 0, 100, null, null, CancellationToken.None);
        scope.Complete();

        Assert.Multiple(() =>
        {
            Assert.That(children.Items.Select(c => c.Key), Does.Contain(regularChild.Key));
            Assert.That(children.Items.Select(c => c.Key), Does.Not.Contain(blueprint.Key),
                "a document blueprint must not be returned by a plain document repository's child query");
        });
    }
}
