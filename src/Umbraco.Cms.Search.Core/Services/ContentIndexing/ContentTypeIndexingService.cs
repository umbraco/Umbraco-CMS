using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

internal sealed class ContentTypeIndexingService : IContentTypeIndexingService
{
    private readonly IContentIndexingService _contentIndexingService;
    private readonly IIndexDocumentService _indexDocumentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IContentService _contentService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMediaService _mediaService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly IMemberService _memberService;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public ContentTypeIndexingService(
        IContentIndexingService contentIndexingService,
        IIndexDocumentService indexDocumentService,
        IContentTypeService contentTypeService,
        IContentService contentService,
        IMediaTypeService mediaTypeService,
        IMediaService mediaService,
        IMemberTypeService memberTypeService,
        IMemberService memberService,
        IBackgroundTaskQueue backgroundTaskQueue)
    {
        _contentIndexingService = contentIndexingService;
        _indexDocumentService = indexDocumentService;
        _contentTypeService = contentTypeService;
        _contentService = contentService;
        _mediaTypeService = mediaTypeService;
        _mediaService = mediaService;
        _memberTypeService = memberTypeService;
        _memberService = memberService;
        _backgroundTaskQueue = backgroundTaskQueue;
    }

    public void ReindexByContentTypes(Guid[] contentTypeKeys, UmbracoObjectTypes objectType, string origin)
        => _backgroundTaskQueue.QueueBackgroundWorkItem(async _ =>
        {
            Guid[] contentKeys = GetContentKeysByContentTypes(contentTypeKeys, objectType);
            if (contentKeys.Length == 0)
            {
                return;
            }

            await FlushDocumentIndexCacheAsync(contentKeys);

            ContentChange[] changes = CreateContentChanges(contentKeys, objectType);
            _contentIndexingService.Handle(changes, origin);
        });

    private Guid[] GetContentKeysByContentTypes(Guid[] contentTypeKeys, UmbracoObjectTypes objectType)
        => objectType switch
        {
            UmbracoObjectTypes.Document => GetDocumentKeysByContentTypes(contentTypeKeys),
            UmbracoObjectTypes.Media => GetMediaKeysByMediaTypes(contentTypeKeys),
            UmbracoObjectTypes.Member => GetMemberKeysByMemberTypes(contentTypeKeys),
            _ => [],
        };

    private Guid[] GetDocumentKeysByContentTypes(Guid[] contentTypeKeys)
    {
        int[] directContentTypeIds = contentTypeKeys
            .Select(key => _contentTypeService.Get(key))
            .Where(ct => ct is not null)
            .Select(ct => ct!.Id)
            .ToArray();

        if (directContentTypeIds.Length == 0)
        {
            return [];
        }

        int[] allContentTypeIds = ExpandWithDependentContentTypes(_contentTypeService, directContentTypeIds);

        var keys = new List<Guid>();
        var pageIndex = 0L;

        while (true)
        {
            IContent[] page = _contentService.GetPagedOfTypes(
                allContentTypeIds, pageIndex, 1000, out long totalRecords, null, null).ToArray();
            keys.AddRange(page.Select(c => c.Key));
            pageIndex++;

            if (keys.Count >= totalRecords)
            {
                break;
            }
        }

        return keys.ToArray();
    }

    private Guid[] GetMediaKeysByMediaTypes(Guid[] mediaTypeKeys)
    {
        int[] directMediaTypeIds = mediaTypeKeys
            .Select(key => _mediaTypeService.Get(key))
            .Where(mt => mt is not null)
            .Select(mt => mt!.Id)
            .ToArray();

        if (directMediaTypeIds.Length == 0)
        {
            return [];
        }

        int[] allMediaTypeIds = ExpandWithDependentContentTypes(_mediaTypeService, directMediaTypeIds);

        var keys = new List<Guid>();
        var pageIndex = 0L;

        while (true)
        {
            IMedia[] page = _mediaService.GetPagedOfTypes(
                allMediaTypeIds, pageIndex, 1000, out long totalRecords, null, null).ToArray();
            keys.AddRange(page.Select(m => m.Key));
            pageIndex++;

            if (keys.Count >= totalRecords)
            {
                break;
            }
        }

        return keys.ToArray();
    }

    private Guid[] GetMemberKeysByMemberTypes(Guid[] memberTypeKeys)
    {
        int[] directMemberTypeIds = memberTypeKeys
            .Select(key => _memberTypeService.Get(key))
            .Where(mt => mt is not null)
            .Select(mt => mt!.Id)
            .ToArray();

        if (directMemberTypeIds.Length == 0)
        {
            return [];
        }

        int[] allMemberTypeIds = ExpandWithDependentContentTypes(_memberTypeService, directMemberTypeIds);

        var keys = new List<Guid>();
        foreach (int memberTypeId in allMemberTypeIds)
        {
            IEnumerable<IMember> members = _memberService.GetMembersByMemberType(memberTypeId);
            keys.AddRange(members.Select(m => m.Key));
        }

        return keys.ToArray();
    }

    private static int[] ExpandWithDependentContentTypes<T>(IContentTypeBaseService<T> contentTypeService, int[] contentTypeIds)
        where T : IContentTypeComposition
    {
        T[] allTypes = contentTypeService.GetAll().ToArray();
        var result = new HashSet<int>(contentTypeIds);

        int previousCount;
        do
        {
            previousCount = result.Count;
            foreach (T ct in allTypes)
            {
                if (result.Contains(ct.Id) is false && ct.CompositionIds().Any(result.Contains))
                {
                    result.Add(ct.Id);
                }
            }
        }
        while (result.Count > previousCount);

        return result.ToArray();
    }

    private async Task FlushDocumentIndexCacheAsync(Guid[] contentKeys)
    {
        await _indexDocumentService.DeleteAsync(contentKeys, true);
        await _indexDocumentService.DeleteAsync(contentKeys, false);
    }

    private static ContentChange[] CreateContentChanges(Guid[] contentKeys, UmbracoObjectTypes objectType)
        => objectType switch
        {
            UmbracoObjectTypes.Document => contentKeys
                .SelectMany(key => new[]
                {
                    ContentChange.Document(key, ChangeImpact.Refresh, ContentState.Draft),
                    ContentChange.Document(key, ChangeImpact.Refresh, ContentState.Published),
                })
                .ToArray(),
            UmbracoObjectTypes.Media => contentKeys
                .Select(key => ContentChange.Media(key, ChangeImpact.Refresh, ContentState.Draft))
                .ToArray(),
            UmbracoObjectTypes.Member => contentKeys
                .Select(key => ContentChange.Member(key, ChangeImpact.Refresh, ContentState.Draft))
                .ToArray(),
            _ => [],
        };
}
