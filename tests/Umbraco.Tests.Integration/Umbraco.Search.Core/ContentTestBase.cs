using Umbraco.Cms.Core.Models;
using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public abstract class ContentTestBase : ContentBaseTestBase
{
    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected IContentService ContentService => GetRequiredService<IContentService>();

    protected IContentIndexingService ContentIndexingService => GetRequiredService<IContentIndexingService>();

    protected Guid RootKey { get; } = Guid.NewGuid();

    protected Guid ChildKey { get; } = Guid.NewGuid();

    protected Guid GrandchildKey { get; } = Guid.NewGuid();

    protected Guid GreatGrandchildKey { get; } = Guid.NewGuid();

    protected IContent Root() => ContentService.GetById(RootKey) ?? throw new InvalidOperationException("Root was not found");

    protected IContent Child() => ContentService.GetById(ChildKey) ?? throw new InvalidOperationException("Child was not found");

    protected IContent Grandchild() => ContentService.GetById(GrandchildKey) ?? throw new InvalidOperationException("Grandchild was not found");

    protected IContent GreatGrandchild() => ContentService.GetById(GreatGrandchildKey) ?? throw new InvalidOperationException("Great grandchild was not found");
}
