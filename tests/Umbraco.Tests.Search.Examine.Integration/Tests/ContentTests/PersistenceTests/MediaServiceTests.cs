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
public class MediaServiceTests : UmbracoIntegrationTestWithPackageMigrations
{
    private bool _indexingComplete;

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();
    private IIndexDocumentRepository IndexDocumentRepository => GetRequiredService<IIndexDocumentRepository>();

    private IMedia _rootMedia = null!;

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
        IndexDocument? doc = await IndexDocumentRepository.GetAsync(_rootMedia.Key, false);
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
            IndexDocument? initialDoc = await IndexDocumentRepository.GetAsync(_rootMedia.Key, false);
            Assert.That(initialDoc, Is.Not.Null);
            initialFields = initialDoc!.Fields;
        }

        // Update the media name
        _rootMedia.Name = "Updated Root Media";

        await WaitForIndexing(Constants.IndexAliases.DraftMedia, () =>
        {
            MediaService.Save(_rootMedia);
            return Task.CompletedTask;
        });

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify the document was updated
            IndexDocument? updatedDoc = await IndexDocumentRepository.GetAsync(_rootMedia.Key, false);
            Assert.That(updatedDoc, Is.Not.Null);
            Assert.That(updatedDoc!.Fields, Is.Not.EqualTo(initialFields));
            Assert.That(FieldsContainText(updatedDoc.Fields, "Updated Root Media"), Is.True);
        }
    }

    [Test]
    public async Task RemovesEntryFromDatabaseAfterDeletion()
    {
        await TestSetup();

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify initial document exists
            IndexDocument? initialDoc = await IndexDocumentRepository.GetAsync(_rootMedia.Key, false);
            Assert.That(initialDoc, Is.Not.Null);
        }

        // Delete the media
        await WaitForIndexing(Constants.IndexAliases.DraftMedia, () =>
        {
            MediaService.Delete(_rootMedia);
            return Task.CompletedTask;
        });

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // Verify the document was removed
            IndexDocument? deletedDoc = await IndexDocumentRepository.GetAsync(_rootMedia.Key, false);
            Assert.That(deletedDoc, Is.Null);
        }
    }

    private async Task TestSetup()
    {
        IMediaType mediaType = new MediaTypeBuilder()
            .WithAlias("testMediaType")
            .AddPropertyGroup()
            .AddPropertyType()
            .WithAlias("altText")
            .WithDataTypeId(Umbraco.Cms.Core.Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Done()
            .Build();
        await MediaTypeService.CreateAsync(mediaType, Umbraco.Cms.Core.Constants.Security.SuperUserKey);

        await WaitForIndexing(Constants.IndexAliases.DraftMedia, () =>
        {
            _rootMedia = new MediaBuilder()
                .WithMediaType(mediaType)
                .WithName("Root Media")
                .WithPropertyValues(new { altText = "The media alt text" })
                .Build();
            MediaService.Save(_rootMedia);
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
