using Umbraco.Cms.Tests.Integration.Testing;
using System.Diagnostics;
using System.Reflection;
using Examine;
using Examine.Lucene.Providers;
using NUnit.Framework;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Search.Core.Cache.Language;
using Umbraco.Cms.Search.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Models.Persistence;
using Umbraco.Cms.Search.Core.NotificationHandlers;
using Umbraco.Cms.Search.Core.Persistence;
using Umbraco.Cms.Search.Provider.Examine.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Tests.Search.Examine.Integration.Attributes;
using Umbraco.Tests.Search.Examine.Integration.Extensions;
using Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.IndexService;
using Umbraco.Tests.Search.Integration;
using Constants = Umbraco.Cms.Search.Core.Constants;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.PersistenceTests;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class MemberServiceTests : UmbracoIntegrationTest
{
    private bool _indexingComplete;

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IIndexDocumentRepository IndexDocumentRepository => GetRequiredService<IIndexDocumentRepository>();

    private IMember _member = null!;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.AddExamineSearchProviderForTest<TestIndex, TestInMemoryDirectoryFactory>();

        builder.AddSearchCore();

        builder.Services.AddUnique<IBackgroundTaskQueue, ImmediateBackgroundTaskQueue>();
        builder.Services.AddUnique<IServerMessenger, LocalServerMessenger>();
        builder.Services.AddUnique<IServerEventRouter, NoOpServerEventRouter>();
        builder.AddNotificationHandler<LanguageCacheRefresherNotification, RebuildIndexesNotificationHandler>();

        // the core ConfigureBuilderAttribute won't execute from other assemblies at the moment, so this is a workaround
        var testType = Type.GetType(TestContext.CurrentContext.Test.ClassName!);
        if (testType is not null)
        {
            MethodInfo? methodInfo = testType.GetMethod(TestContext.CurrentContext.Test.Name);
            if (methodInfo is not null)
            {
                foreach (ConfigureUmbracoBuilderAttribute attribute in methodInfo.GetCustomAttributes(typeof(ConfigureUmbracoBuilderAttribute), true).OfType<ConfigureUmbracoBuilderAttribute>())
                {
                    attribute.Execute(builder, testType);
                }
            }
        }
    }

    [Test]
    public async Task AddsEntryToDatabaseAfterIndexing()
    {
        await TestSetup();
        using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
        IndexDocument? doc = await IndexDocumentRepository.GetAsync(_member.Key, false);
        Assert.That(doc, Is.Not.Null);
    }

    [Test]
    public async Task UpdatesEntryInDatabaseAfterPropertyChange()
    {
        await TestSetup();

        IndexField[] initialFields;
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify initial document exists
            IndexDocument? initialDoc = await IndexDocumentRepository.GetAsync(_member.Key, false);
            Assert.That(initialDoc, Is.Not.Null);
            initialFields = initialDoc!.Fields;
        }

        // Update the member name
        _member.Name = "Updated Member Name";

        await WaitForIndexing(Umbraco.Cms.Core.Constants.IndexAliases.DraftMembers, () =>
        {
            MemberService.Save(_member);
            return Task.CompletedTask;
        });

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify the document was updated
            IndexDocument? updatedDoc = await IndexDocumentRepository.GetAsync(_member.Key, false);
            Assert.That(updatedDoc, Is.Not.Null);
            Assert.That(updatedDoc!.Fields, Is.Not.EqualTo(initialFields));
            Assert.That(FieldsContainText(updatedDoc.Fields, "Updated Member Name"), Is.True);
        }
    }

    [Test]
    public async Task RemovesEntryFromDatabaseAfterDeletion()
    {
        await TestSetup();

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify initial document exists
            IndexDocument? initialDoc = await IndexDocumentRepository.GetAsync(_member.Key, false);
            Assert.That(initialDoc, Is.Not.Null);
        }

        // Delete the member
        MemberService.Delete(_member);
        await Task.Delay(4000);

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify the document was removed
            IndexDocument? deletedDoc = await IndexDocumentRepository.GetAsync(_member.Key, false);
            Assert.That(deletedDoc, Is.Null);
        }
    }

    private async Task TestSetup()
    {
        IMemberType memberType = new MemberTypeBuilder()
            .WithAlias("testMemberType")
            .AddPropertyGroup()
            .AddPropertyType()
            .WithAlias("organization")
            .WithDataTypeId(Umbraco.Cms.Core.Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Done()
            .Build();
        await MemberTypeService.CreateAsync(memberType, Umbraco.Cms.Core.Constants.Security.SuperUserKey);

        await WaitForIndexing(Umbraco.Cms.Core.Constants.IndexAliases.DraftMembers, () =>
        {
            _member = new MemberBuilder()
                .WithMemberType(memberType)
                .WithName("Test Member")
                .WithEmail("testmember@local")
                .WithLogin("testmember@local", "Test123456")
                .AddPropertyData()
                .WithKeyValue("organization", "Test Organization")
                .Done()
                .Build();
            MemberService.Save(_member);
            return Task.CompletedTask;
        });
    }

    private async Task WaitForIndexing(string indexAlias, Func<Task> indexUpdatingAction)
    {
        var activeIndexManager = GetRequiredService<IActiveIndexManager>();
        var physicalName = activeIndexManager.IsRebuilding(indexAlias)
            ? activeIndexManager.ResolveShadowIndexName(indexAlias)
            : activeIndexManager.ResolveActiveIndexName(indexAlias);
        var index = (LuceneIndex)GetRequiredService<IExamineManager>().GetIndex(physicalName);
        index.IndexCommitted += IndexCommited;

        var hasDoneAction = false;

        var stopWatch = Stopwatch.StartNew();

        while (_indexingComplete is false)
        {
            if (hasDoneAction is false)
            {
                await indexUpdatingAction();
                hasDoneAction = true;
            }

            if (stopWatch.ElapsedMilliseconds > 5000)
            {
                throw new TimeoutException("Indexing timed out");
            }

            await Task.Delay(250);
        }

        _indexingComplete = false;
        index.IndexCommitted -= IndexCommited;
    }

    private void IndexCommited(object? sender, EventArgs e) => _indexingComplete = true;

    private static bool FieldsContainText(IndexField[] fields, string text)
        => fields.Any(f =>
            (f.Value.Texts?.Any(t => t.Contains(text)) == true) ||
            (f.Value.TextsR1?.Any(t => t.Contains(text)) == true) ||
            (f.Value.TextsR2?.Any(t => t.Contains(text)) == true) ||
            (f.Value.TextsR3?.Any(t => t.Contains(text)) == true) ||
            (f.Value.Keywords?.Any(k => k.Contains(text)) == true));
}
