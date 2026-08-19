using NUnit.Framework;
using Umbraco.Cms.Core.Models;
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

    protected IContent Root() => ContentService.GetByIdAsync(RootKey, CancellationToken.None).GetAwaiter().GetResult() ?? throw new InvalidOperationException("Root was not found");

    protected IContent Child() => ContentService.GetByIdAsync(ChildKey, CancellationToken.None).GetAwaiter().GetResult() ?? throw new InvalidOperationException("Child was not found");

    protected IContent Grandchild() => ContentService.GetByIdAsync(GrandchildKey, CancellationToken.None).GetAwaiter().GetResult() ?? throw new InvalidOperationException("Grandchild was not found");

    protected IContent GreatGrandchild() => ContentService.GetByIdAsync(GreatGrandchildKey, CancellationToken.None).GetAwaiter().GetResult() ?? throw new InvalidOperationException("Great grandchild was not found");
}
