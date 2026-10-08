// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ContentNavigationRepositoryTest : UmbracoIntegrationTestWithContent
{
    [Test]
    public async Task Can_Get_Node_With_Ancestors_Ordered_From_The_Root()
    {
        Content grandchild = ContentBuilder.CreateSimpleContent(ContentType, "Grandchild", Subpage.Id);
        await ContentService.SaveAsync(grandchild, Constants.Security.SuperUserKey, null, CancellationToken.None);

        using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
        INavigationRepository repository = new ContentNavigationRepository((IScopeAccessor)ScopeProvider);

        List<INavigationModel> chain = repository.GetContentNodeWithAncestors(grandchild.Key, Constants.ObjectTypes.Document).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(chain.Select(x => x.Key), Is.EqualTo(new[] { Textpage.Key, Subpage.Key, grandchild.Key }));
            Assert.That(chain.Select(x => x.ParentId), Is.EqualTo(new[] { Constants.System.Root, Textpage.Id, Subpage.Id }));
            Assert.That(chain.Select(x => x.Id), Is.EqualTo(new[] { Textpage.Id, Subpage.Id, grandchild.Id }));
            Assert.That(chain.Select(x => x.ContentTypeKey), Is.All.EqualTo(ContentType.Key));
            Assert.That(chain.Select(x => x.SortOrder), Is.EqualTo(new[] { Textpage.SortOrder, Subpage.SortOrder, grandchild.SortOrder }));
            Assert.That(chain.Select(x => x.Trashed), Is.All.False);
        });
    }

    [Test]
    public void Trashed_Nodes_Are_Flagged_And_Their_Chain_Stops_At_The_Recycle_Bin()
    {
        using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
        INavigationRepository repository = new ContentNavigationRepository((IScopeAccessor)ScopeProvider);

        List<INavigationModel> chain = repository.GetContentNodeWithAncestors(Trashed.Key, Constants.ObjectTypes.Document).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(chain.Select(x => x.Key), Is.EqualTo(new[] { Trashed.Key }));
            Assert.That(chain.Single().Trashed, Is.True);
            Assert.That(chain.Single().ParentId, Is.EqualTo(Constants.System.RecycleBinContent));
        });
    }

    [Test]
    public void Returns_Nothing_For_An_Unknown_Key_Or_Another_Object_Type()
    {
        using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
        INavigationRepository repository = new ContentNavigationRepository((IScopeAccessor)ScopeProvider);

        Assert.Multiple(() =>
        {
            Assert.That(repository.GetContentNodeWithAncestors(Guid.NewGuid(), Constants.ObjectTypes.Document), Is.Empty);
            Assert.That(repository.GetContentNodeWithAncestors(Subpage.Key, Constants.ObjectTypes.Media), Is.Empty);
        });
    }

    [Test]
    public void Returns_Nothing_Without_An_Ambient_Scope()
    {
        INavigationRepository repository = new ContentNavigationRepository((IScopeAccessor)ScopeProvider);

        Assert.That(repository.GetContentNodeWithAncestors(Subpage.Key, Constants.ObjectTypes.Document), Is.Empty);
    }
}
