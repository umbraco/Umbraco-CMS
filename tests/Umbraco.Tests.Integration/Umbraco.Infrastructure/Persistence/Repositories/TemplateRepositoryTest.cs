// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class TemplateRepositoryTest : UmbracoIntegrationTest
{
    private FileSystems FileSystems => GetRequiredService<FileSystems>();

    private CountingDbCommandInterceptor CommandCounter => GetRequiredService<CountingDbCommandInterceptor>();

    [SetUp]
    public void SetUp() => DeleteAllTemplateViewFiles();

    [TearDown]
    public void TearDown() => DeleteAllTemplateViewFiles();

    /// <summary>
    ///     Attaches the command counter so tests can assert on query cost. Registered here rather than in
    ///     <c>CustomTestSetup</c> because this hook runs last, after the harness has rewrapped the same descriptor
    ///     for its own reasons.
    /// </summary>
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddSingleton<CountingDbCommandInterceptor>();

        ServiceDescriptor descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<UmbracoDbContext>));
        Func<IServiceProvider, object> originalFactory = descriptor.ImplementationFactory!;
        services.Remove(descriptor);
        services.AddSingleton<DbContextOptions<UmbracoDbContext>>(serviceProvider =>
        {
            var options = (DbContextOptions<UmbracoDbContext>)originalFactory(serviceProvider);
            return new DbContextOptionsBuilder<UmbracoDbContext>(options)
                .AddInterceptors(serviceProvider.GetRequiredService<CountingDbCommandInterceptor>())
                .Options;
        });
    }

    [Test]
    public async Task Can_Perform_Add()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var template = new Template(ShortStringHelper, "test", "test");
        await repository.SaveAsync(template, CancellationToken.None);

        Assert.That(template.HasIdentity, Is.True);
        Assert.That(template.Path, Is.EqualTo($"-1,{template.Id}"));

        ITemplate? persisted = await repository.GetAsync(template.Key, CancellationToken.None);
        Assert.That(persisted, Is.Not.Null);
        Assert.That(persisted!.Id, Is.EqualTo(template.Id));
        Assert.That(persisted.Name, Is.EqualTo("test"));
        Assert.That(persisted.Alias, Is.EqualTo("test"));
        scope.Complete();
    }

    [Test]
    public async Task Save_Does_Not_Write_View_File()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var template = new Template(ShortStringHelper, "noFileTest", "noFileTest") { Content = "mock-content" };
        await repository.SaveAsync(template, CancellationToken.None);

        Assert.That(await repository.GetByAliasAsync("noFileTest", CancellationToken.None), Is.Not.Null);
        Assert.That(FileSystems.MvcViewsFileSystem!.FileExists("noFileTest.cshtml"), Is.False);
        scope.Complete();
    }

    [Test]
    public async Task Can_Perform_Add_Unique_Alias()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var template = new Template(ShortStringHelper, "test", "test");
        await repository.SaveAsync(template, CancellationToken.None);

        var template2 = new Template(ShortStringHelper, "test", "test");
        await repository.SaveAsync(template2, CancellationToken.None);

        var template3 = new Template(ShortStringHelper, "test", "test");
        await repository.SaveAsync(template3, CancellationToken.None);

        Assert.That(template2.Alias, Is.EqualTo("test1"));
        Assert.That(template3.Alias, Is.EqualTo("test2"));
        scope.Complete();
    }

    [Test]
    public async Task Can_Perform_Update_Unique_Alias()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var template = new Template(ShortStringHelper, "test", "test");
        await repository.SaveAsync(template, CancellationToken.None);

        var template2 = new Template(ShortStringHelper, "test1", "test1");
        await repository.SaveAsync(template2, CancellationToken.None);

        template.Alias = "test1";
        await repository.SaveAsync(template, CancellationToken.None);

        Assert.That(template.Alias, Is.EqualTo("test11"));
        ITemplate? persisted = await repository.GetAsync(template.Key, CancellationToken.None);
        Assert.That(persisted!.Alias, Is.EqualTo("test11"));
        scope.Complete();
    }

    [Test]
    public async Task Over_Long_Alias_Is_Truncated_And_Kept_Unique()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        var longAlias = new string('a', 120);
        var truncatedAlias = new string('a', 95);

        var template = new Template(ShortStringHelper, "long", longAlias);
        await repository.SaveAsync(template, CancellationToken.None);

        var template2 = new Template(ShortStringHelper, "long", longAlias);
        await repository.SaveAsync(template2, CancellationToken.None);

        Assert.That(template.Alias, Is.EqualTo(truncatedAlias));
        Assert.That(template2.Alias, Is.EqualTo(truncatedAlias + "1"));
        Assert.That((await repository.GetAsync(template2.Key, CancellationToken.None))!.Alias, Is.EqualTo(truncatedAlias + "1"));
        scope.Complete();
    }

    [Test]
    public async Task Unique_Alias_Ignores_Case()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(new Template(ShortStringHelper, "test", "test"), CancellationToken.None);

        var template = new Template(ShortStringHelper, "Test", "Test");
        await repository.SaveAsync(template, CancellationToken.None);

        Assert.That(template.Alias, Is.EqualTo("test1").IgnoreCase);
        scope.Complete();
    }

    [Test]
    public async Task Unique_Alias_Skips_Suffixes_Already_Taken()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        await repository.SaveAsync(new Template(ShortStringHelper, "test", "test"), CancellationToken.None);
        await repository.SaveAsync(new Template(ShortStringHelper, "test2", "test2"), CancellationToken.None);
        await repository.SaveAsync(new Template(ShortStringHelper, "test1", "test1"), CancellationToken.None);

        var template = new Template(ShortStringHelper, "test", "test");
        await repository.SaveAsync(template, CancellationToken.None);

        Assert.That(template.Alias, Is.EqualTo("test3"));
        scope.Complete();
    }

    [Test]
    public async Task Can_Perform_Update()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var template = new Template(ShortStringHelper, "test", "test");
        await repository.SaveAsync(template, CancellationToken.None);

        template.Name = "Updated name";
        template.Alias = "updatedAlias";
        await repository.SaveAsync(template, CancellationToken.None);

        ITemplate? persisted = await repository.GetAsync(template.Key, CancellationToken.None);
        Assert.That(persisted, Is.Not.Null);
        Assert.That(persisted!.Name, Is.EqualTo("Updated name"));
        Assert.That(persisted.Alias, Is.EqualTo("updatedAlias"));
        Assert.That(await repository.GetByAliasAsync("test", CancellationToken.None), Is.Null);
        scope.Complete();
    }

    [Test]
    public async Task Can_Perform_Delete()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var template = new Template(ShortStringHelper, "test", "test");
        await repository.SaveAsync(template, CancellationToken.None);

        await repository.DeleteAsync(template, CancellationToken.None);

        Assert.That(await repository.GetAsync(template.Key, CancellationToken.None), Is.Null);
        Assert.That(await repository.ExistsAsync(template.Key, CancellationToken.None), Is.False);
        var nodeExists = await scope.ExecuteWithContextAsync(db => db.Nodes.AnyAsync(node => node.NodeId == template.Id));
        Assert.That(nodeExists, Is.False);
        scope.Complete();
    }

    [Test]
    public async Task Can_Perform_Delete_When_Assigned_To_Content_Type_And_Document()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await GetRequiredService<ITemplateService>().CreateAsync(template, Constants.Security.SuperUserKey, CancellationToken.None);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage2", "Textpage", defaultTemplateId: template.Id);
        await GetRequiredService<IContentTypeService>().CreateAsync(contentType, Constants.Security.SuperUserKey);

        var textPage = ContentBuilder.CreateSimpleContent(contentType);
        textPage.TemplateId = template.Id;
        await GetRequiredService<IContentService>().SaveAsync(textPage, Constants.Security.SuperUserKey, null, CancellationToken.None);

        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var assignedVersions = await scope.ExecuteWithContextAsync(db =>
            db.DocumentVersions.CountAsync(documentVersion => documentVersion.TemplateId == template.Id));
        var assignedContentTypes = await scope.ExecuteWithContextAsync(db =>
            db.ContentTypeTemplates.CountAsync(contentTypeTemplate => contentTypeTemplate.TemplateNodeId == template.Id));
        Assert.That(assignedVersions, Is.GreaterThan(0));
        Assert.That(assignedContentTypes, Is.GreaterThan(0));

        ITemplate? persisted = await repository.GetByAliasAsync("textPage", CancellationToken.None);
        await repository.DeleteAsync(persisted!, CancellationToken.None);

        Assert.That(await repository.GetByAliasAsync("textPage", CancellationToken.None), Is.Null);
        assignedVersions = await scope.ExecuteWithContextAsync(db =>
            db.DocumentVersions.CountAsync(documentVersion => documentVersion.TemplateId == template.Id));
        assignedContentTypes = await scope.ExecuteWithContextAsync(db =>
            db.ContentTypeTemplates.CountAsync(contentTypeTemplate => contentTypeTemplate.TemplateNodeId == template.Id));
        Assert.That(assignedVersions, Is.EqualTo(0));
        Assert.That(assignedContentTypes, Is.EqualTo(0));
        scope.Complete();
    }

    [Test]
    public async Task Can_Perform_Delete_On_Nested_Templates()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var parent = new Template(ShortStringHelper, "parent", "parent");
        await repository.SaveAsync(parent, CancellationToken.None);
        var child = new Template(ShortStringHelper, "child", "child");
        SetLayout(child, parent);
        await repository.SaveAsync(child, CancellationToken.None);
        var baby = new Template(ShortStringHelper, "baby", "baby");
        SetLayout(baby, child);
        await repository.SaveAsync(baby, CancellationToken.None);

        await repository.DeleteAsync(parent, CancellationToken.None);

        Assert.That(await repository.GetAllAsync(CancellationToken.None), Is.Empty);
        var remainingNodes = await scope.ExecuteWithContextAsync(db =>
            db.Nodes.CountAsync(node => node.NodeObjectType == Constants.ObjectTypes.Template));
        Assert.That(remainingNodes, Is.EqualTo(0));
        scope.Complete();
    }

    [Test]
    public async Task Can_Get_All()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        await CreateHierarchyAsync(repository);

        ITemplate[] all = (await repository.GetAllAsync(CancellationToken.None)).ToArray();

        Assert.That(all, Has.Length.EqualTo(9));
        Assert.That(all.DistinctBy(template => template.Key).Count(), Is.EqualTo(9));
        scope.Complete();
    }

    [Test]
    public async Task Can_Get_Many_By_Key()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        ITemplate[] created = await CreateHierarchyAsync(repository);

        ITemplate[] many = (await repository.GetManyAsync(
            [created[0].Key, created[2].Key, created[4].Key, created[5].Key, Guid.NewGuid()],
            CancellationToken.None)).ToArray();

        Assert.That(many.Select(template => template.Key), Is.EquivalentTo(new[] { created[0].Key, created[2].Key, created[4].Key, created[5].Key }));
        scope.Complete();
    }

    [Test]
    public async Task Get_By_Alias_Is_Case_Insensitive()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var template = new Template(ShortStringHelper, "aliasLookupTest", "aliasLookupTest");
        await repository.SaveAsync(template, CancellationToken.None);

        ITemplate? result = await repository.GetByAliasAsync("ALIASLOOKUPTEST", CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Key, Is.EqualTo(template.Key));
        scope.Complete();
    }

    [Test]
    public async Task Exists_By_Key_Returns_Correct_Result()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var template = new Template(ShortStringHelper, "existsTest", "existsTest");
        await repository.SaveAsync(template, CancellationToken.None);

        Assert.That(await repository.ExistsAsync(template.Key, CancellationToken.None), Is.True);
        Assert.That(await repository.ExistsAsync(Guid.NewGuid(), CancellationToken.None), Is.False);
        scope.Complete();
    }

    [Test]
    public async Task Layout_Relationship_Is_Mapped_On_Read()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var parent = new Template(ShortStringHelper, "parent", "parent");
        await repository.SaveAsync(parent, CancellationToken.None);
        var child = new Template(ShortStringHelper, "child", "child");
        SetLayout(child, parent);
        await repository.SaveAsync(child, CancellationToken.None);

        var persistedParent = (Template)(await repository.GetAsync(parent.Key, CancellationToken.None))!;
        var persistedChild = (Template)(await repository.GetAsync(child.Key, CancellationToken.None))!;

        Assert.That(persistedParent.IsLayoutTemplate, Is.True);
        Assert.That(persistedParent.LayoutTemplateAlias, Is.Null);
        Assert.That(persistedChild.IsLayoutTemplate, Is.False);
        Assert.That(persistedChild.LayoutTemplateAlias, Is.EqualTo("parent"));
        Assert.That(persistedChild.LayoutTemplateId!.Value, Is.EqualTo(parent.Id));
        scope.Complete();
    }

    [Test]
    public async Task Can_Get_Descendants()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        ITemplate[] created = await CreateHierarchyAsync(repository);

        ITemplate[] descendants = (await repository.GetDescendantsAsync(created[1].Key, CancellationToken.None)).ToArray();

        Assert.That(descendants.Select(template => template.Alias), Is.EquivalentTo(new[] { "toddler1", "toddler2", "baby1" }));
        scope.Complete();
    }

    [Test]
    public async Task Can_Get_Descendants_From_Root()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        await CreateHierarchyAsync(repository);

        ITemplate[] descendants = (await repository.GetDescendantsAsync(null, CancellationToken.None)).ToArray();

        Assert.That(descendants, Has.Length.EqualTo(9));
        Assert.That(descendants[0].Alias, Is.EqualTo("parent"));
        scope.Complete();
    }

    [Test]
    public async Task Get_Descendants_Of_Unknown_Template_Returns_Empty()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();
        await CreateHierarchyAsync(repository);

        IEnumerable<ITemplate> descendants = await repository.GetDescendantsAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(descendants, Is.Empty);
        scope.Complete();
    }

    [Test]
    public async Task Path_Is_Set_Correctly_On_Creation()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        ITemplate[] created = await CreateHierarchyAsync(repository);
        Template parent = (Template)created[0], child1 = (Template)created[1], child2 = (Template)created[2],
            toddler1 = (Template)created[3], toddler2 = (Template)created[4], toddler3 = (Template)created[5],
            toddler4 = (Template)created[6], baby1 = (Template)created[7], baby2 = (Template)created[8];

        Assert.That(parent.Path, Is.EqualTo($"-1,{parent.Id}"));
        Assert.That(child1.Path, Is.EqualTo($"-1,{parent.Id},{child1.Id}"));
        Assert.That(child2.Path, Is.EqualTo($"-1,{parent.Id},{child2.Id}"));
        Assert.That(toddler1.Path, Is.EqualTo($"-1,{parent.Id},{child1.Id},{toddler1.Id}"));
        Assert.That(toddler2.Path, Is.EqualTo($"-1,{parent.Id},{child1.Id},{toddler2.Id}"));
        Assert.That(toddler3.Path, Is.EqualTo($"-1,{parent.Id},{child2.Id},{toddler3.Id}"));
        Assert.That(toddler4.Path, Is.EqualTo($"-1,{parent.Id},{child2.Id},{toddler4.Id}"));
        Assert.That(baby1.Path, Is.EqualTo($"-1,{parent.Id},{child1.Id},{toddler2.Id},{baby1.Id}"));
        Assert.That(baby2.Path, Is.EqualTo($"-1,{parent.Id},{child2.Id},{toddler4.Id},{baby2.Id}"));

        ITemplate? persistedBaby2 = await repository.GetAsync(baby2.Key, CancellationToken.None);
        Assert.That(persistedBaby2!.Path, Is.EqualTo(baby2.Path));
        scope.Complete();
    }

    [Test]
    public async Task Path_Is_Set_Correctly_On_Update()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var parent = new Template(ShortStringHelper, "parent", "parent");
        await repository.SaveAsync(parent, CancellationToken.None);
        var child1 = new Template(ShortStringHelper, "child1", "child1");
        SetLayout(child1, parent);
        await repository.SaveAsync(child1, CancellationToken.None);
        var child2 = new Template(ShortStringHelper, "child2", "child2");
        SetLayout(child2, parent);
        await repository.SaveAsync(child2, CancellationToken.None);
        var toddler = new Template(ShortStringHelper, "toddler", "toddler");
        SetLayout(toddler, child1);
        await repository.SaveAsync(toddler, CancellationToken.None);

        SetLayout(toddler, child2);
        await repository.SaveAsync(toddler, CancellationToken.None);

        Assert.That(toddler.Path, Is.EqualTo($"-1,{parent.Id},{child2.Id},{toddler.Id}"));
        ITemplate? persisted = await repository.GetAsync(toddler.Key, CancellationToken.None);
        Assert.That(persisted!.Path, Is.EqualTo(toddler.Path));
        Assert.That(persisted.LayoutTemplateAlias, Is.EqualTo("child2"));
        scope.Complete();
    }

    [Test]
    public async Task Path_Is_Set_Correctly_On_Update_With_Layout_Template_Removal()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository();

        var parent = new Template(ShortStringHelper, "parent", "parent");
        await repository.SaveAsync(parent, CancellationToken.None);
        var child = new Template(ShortStringHelper, "child", "child");
        SetLayout(child, parent);
        await repository.SaveAsync(child, CancellationToken.None);

        child.LayoutTemplateAlias = null;
        child.LayoutTemplateId = new Lazy<int>(() => -1);
        await repository.SaveAsync(child, CancellationToken.None);

        Assert.That(child.Path, Is.EqualTo($"-1,{child.Id}"));
        ITemplate? persisted = await repository.GetAsync(child.Key, CancellationToken.None);
        Assert.That(persisted!.Path, Is.EqualTo(child.Path));
        Assert.That(persisted.LayoutTemplateAlias, Is.Null);
        scope.Complete();
    }

    [Test]
    public async Task Retrieval_By_Key_After_Retrieval_By_Key_Is_Cached()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(CreateRealAppCaches());
        ITemplate template = await CreateTemplateAsync(repository);

        var firstRetrievalCount = await CountCommandsAsync(() => repository.GetAsync(template.Key, CancellationToken.None));
        var secondRetrievalCount = await CountCommandsAsync(() => repository.GetAsync(template.Key, CancellationToken.None));

        Assert.That(firstRetrievalCount, Is.GreaterThan(0));
        Assert.That(secondRetrievalCount, Is.EqualTo(0));
        scope.Complete();
    }

    [Test]
    public async Task Retrieval_By_Alias_After_Retrieval_By_Key_Is_Cached()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(CreateRealAppCaches());
        ITemplate template = await CreateTemplateAsync(repository);

        await repository.GetAsync(template.Key, CancellationToken.None);
        var aliasRetrievalCount = await CountCommandsAsync(() => repository.GetByAliasAsync(template.Alias, CancellationToken.None));
        var descendantsRetrievalCount = await CountCommandsAsync(() => repository.GetDescendantsAsync(null, CancellationToken.None));

        Assert.That(aliasRetrievalCount, Is.EqualTo(0));
        Assert.That(descendantsRetrievalCount, Is.EqualTo(0));
        scope.Complete();
    }

    [Test]
    public async Task Retrieval_After_Save_Reloads_Once_Then_Is_Cached()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(CreateRealAppCaches());
        await CreateTemplateAsync(repository);
        await repository.GetAllAsync(CancellationToken.None);

        var template = new Template(ShortStringHelper, "added", "added");
        await repository.SaveAsync(template, CancellationToken.None);

        ITemplate? retrieved = null;
        var firstRetrievalCount = await CountCommandsAsync(async () => retrieved = await repository.GetAsync(template.Key, CancellationToken.None));
        var secondRetrievalCount = await CountCommandsAsync(() => repository.GetAsync(template.Key, CancellationToken.None));

        Assert.That(retrieved, Is.Not.Null);
        Assert.That(firstRetrievalCount, Is.GreaterThan(0));
        Assert.That(secondRetrievalCount, Is.EqualTo(0));
        scope.Complete();
    }

    [Test]
    public async Task Retrieval_By_Key_After_Deletion_Returns_Null()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(CreateRealAppCaches());
        ITemplate template = await CreateTemplateAsync(repository);

        Assert.That(await repository.GetAsync(template.Key, CancellationToken.None), Is.Not.Null);

        await repository.DeleteAsync(template, CancellationToken.None);

        Assert.That(await repository.GetAsync(template.Key, CancellationToken.None), Is.Null);
        scope.Complete();
    }

    [Test]
    public async Task Retrieval_After_Update_Returns_Updated_Template()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(CreateRealAppCaches());
        ITemplate template = await CreateTemplateAsync(repository);
        await repository.GetAsync(template.Key, CancellationToken.None);

        template.Name = "Updated name";
        await repository.SaveAsync(template, CancellationToken.None);

        ITemplate? persisted = await repository.GetAsync(template.Key, CancellationToken.None);
        Assert.That(persisted!.Name, Is.EqualTo("Updated name"));
        scope.Complete();
    }

    /// <summary>
    ///     Verifies that retrieving many templates by key returns all of them when the cache is populated.
    /// </summary>
    /// <remarks>
    ///     Regression test for https://github.com/umbraco/Umbraco-CMS/issues/21756.
    /// </remarks>
    [Test]
    public async Task GetMany_By_Key_With_Warm_Cache_Returns_All()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(CreateRealAppCaches());
        ITemplate[] created = await CreateHierarchyAsync(repository);
        await repository.GetAllAsync(CancellationToken.None);

        ITemplate[] result = (await repository.GetManyAsync([.. created.Select(template => template.Key)], CancellationToken.None)).ToArray();

        Assert.That(result, Has.Length.EqualTo(created.Length));
        scope.Complete();
    }

    [Test]
    public async Task Get_By_Key_Returns_Deep_Clone_Not_Cached_Instance()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(CreateRealAppCaches());
        ITemplate template = await CreateTemplateAsync(repository);

        ITemplate? first = await repository.GetAsync(template.Key, CancellationToken.None);
        ITemplate? second = await repository.GetAsync(template.Key, CancellationToken.None);

        Assert.That(first, Is.Not.Null);
        Assert.That(second, Is.Not.Null);
        Assert.That(second!.Id, Is.EqualTo(first!.Id));
        Assert.That(second, Is.Not.SameAs(first));
        scope.Complete();
    }

    [Test]
    public async Task Get_By_Alias_Returns_Deep_Clone_Not_Cached_Instance()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(CreateRealAppCaches());
        ITemplate template = await CreateTemplateAsync(repository);

        ITemplate? first = await repository.GetByAliasAsync(template.Alias, CancellationToken.None);
        ITemplate? second = await repository.GetByAliasAsync(template.Alias, CancellationToken.None);

        Assert.That(first, Is.Not.Null);
        Assert.That(second, Is.Not.Null);
        Assert.That(second!.Id, Is.EqualTo(first!.Id));
        Assert.That(second, Is.Not.SameAs(first));
        scope.Complete();
    }

    [Test]
    public async Task Get_By_Key_Mutation_Does_Not_Affect_Subsequent_Get()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repository = CreateRepository(CreateRealAppCaches());
        ITemplate template = await CreateTemplateAsync(repository);

        ITemplate? first = await repository.GetAsync(template.Key, CancellationToken.None);
        var originalName = first!.Name;
        first.Name = "MUTATED_" + Guid.NewGuid();

        ITemplate? second = await repository.GetAsync(template.Key, CancellationToken.None);
        Assert.That(second!.Name, Is.EqualTo(originalName), "Mutation of a returned entity should not affect the cached copy");
        scope.Complete();
    }

    private TemplateRepository CreateRepository(AppCaches? appCaches = null) => new(
        GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(),
        appCaches ?? AppCaches.Disabled,
        LoggerFactory.CreateLogger<TemplateRepository>(),
        ShortStringHelper,
        Mock.Of<IRepositoryCacheVersionService>(),
        Mock.Of<ICacheSyncService>());

    /// <summary>
    ///     A cache that actually caches. The default is <see cref="AppCaches.Disabled" />, so a test that means to
    ///     exercise the repository cache policy must opt in explicitly - otherwise it passes whatever the policy does.
    /// </summary>
    private static AppCaches CreateRealAppCaches() => new(
        new DeepCloneAppCache(new ObjectCacheAppCache()),
        new DictionaryAppCache(),
        new IsolatedCaches(_ => new DeepCloneAppCache(new ObjectCacheAppCache())));

    private async Task<int> CountCommandsAsync(Func<Task> action)
    {
        CommandCounter.Enabled = true;
        CommandCounter.Reset();
        try
        {
            await action();
            return CommandCounter.Count;
        }
        finally
        {
            CommandCounter.Enabled = false;
        }
    }

    private static void SetLayout(Template template, ITemplate layoutTemplate)
    {
        template.LayoutTemplateAlias = layoutTemplate.Alias;
        template.LayoutTemplateId = new Lazy<int>(() => layoutTemplate.Id);
    }

    private static async Task<ITemplate> CreateTemplateAsync(ITemplateRepository repository)
    {
        ITemplate template = new TemplateBuilder()
            .WithId(0)
            .WithAlias("testTemplate")
            .WithName("Test Template")
            .Build();

        await repository.SaveAsync(template, CancellationToken.None);
        return template;
    }

    /// <summary>
    ///     Creates parent → (child1 → (toddler1, toddler2 → baby1), child2 → (toddler3, toddler4 → baby2)).
    /// </summary>
    private async Task<ITemplate[]> CreateHierarchyAsync(ITemplateRepository repository)
    {
        var parent = new Template(ShortStringHelper, "parent", "parent");
        var child1 = new Template(ShortStringHelper, "child1", "child1");
        var child2 = new Template(ShortStringHelper, "child2", "child2");
        var toddler1 = new Template(ShortStringHelper, "toddler1", "toddler1");
        var toddler2 = new Template(ShortStringHelper, "toddler2", "toddler2");
        var toddler3 = new Template(ShortStringHelper, "toddler3", "toddler3");
        var toddler4 = new Template(ShortStringHelper, "toddler4", "toddler4");
        var baby1 = new Template(ShortStringHelper, "baby1", "baby1");
        var baby2 = new Template(ShortStringHelper, "baby2", "baby2");

        await repository.SaveAsync(parent, CancellationToken.None);
        SetLayout(child1, parent);
        await repository.SaveAsync(child1, CancellationToken.None);
        SetLayout(child2, parent);
        await repository.SaveAsync(child2, CancellationToken.None);
        SetLayout(toddler1, child1);
        await repository.SaveAsync(toddler1, CancellationToken.None);
        SetLayout(toddler2, child1);
        await repository.SaveAsync(toddler2, CancellationToken.None);
        SetLayout(toddler3, child2);
        await repository.SaveAsync(toddler3, CancellationToken.None);
        SetLayout(toddler4, child2);
        await repository.SaveAsync(toddler4, CancellationToken.None);
        SetLayout(baby1, toddler2);
        await repository.SaveAsync(baby1, CancellationToken.None);
        SetLayout(baby2, toddler4);
        await repository.SaveAsync(baby2, CancellationToken.None);

        return [parent, child1, child2, toddler1, toddler2, toddler3, toddler4, baby1, baby2];
    }
}
