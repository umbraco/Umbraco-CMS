using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Persistence;

public class NuCacheContentService : RepositoryService, INuCacheContentService
{
    private const string NuCacheSerializerKey = "Umbraco.Web.PublishedCache.NuCache.Serializer";
    private readonly IKeyValueService _keyValueService;
    private readonly ILogger<NuCacheContentService> _logger;
    private readonly IOptions<NuCacheSettings> _nucacheSettings;
    private readonly IProfilingLogger _profilingLogger;
    private readonly INuCacheContentRepository _repository;

    public NuCacheContentService(
        INuCacheContentRepository repository,
        IKeyValueService keyValueService,
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IProfilingLogger profilingLogger,
        IEventMessagesFactory eventMessagesFactory,
        IOptions<NuCacheSettings> nucacheSettings)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _repository = repository;
        _keyValueService = keyValueService;
        _profilingLogger = profilingLogger;
        _nucacheSettings = nucacheSettings;
        _logger = loggerFactory.CreateLogger<NuCacheContentService>();
    }

    public void RebuildDatabaseCacheIfSerializerChanged()
    {
        NuCacheSerializerType serializer = _nucacheSettings.Value.NuCacheSerializerType;
        var currentSerializerValue = _keyValueService.GetValue(NuCacheSerializerKey);

        if (!Enum.TryParse(currentSerializerValue, out NuCacheSerializerType currentSerializer)
            || serializer != currentSerializer)
        {
            _logger.LogWarning(
                "Database NuCache was serialized using {CurrentSerializer}. Currently configured NuCache serializer {Serializer}. Rebuilding Nucache",
                currentSerializer, serializer);

            using (_profilingLogger.TraceDuration<NuCacheContentService>(
                       $"Rebuilding NuCache database with {serializer} serializer"))
            {
                RebuildAll();
                _keyValueService.SetValue(NuCacheSerializerKey, serializer.ToString());
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<ContentNodeKit> GetAllContentSources()
        => _repository.GetAllContentSources();

    /// <inheritdoc />
    public IEnumerable<ContentNodeKit> GetAllMediaSources()
        => _repository.GetAllMediaSources();

    /// <inheritdoc />
    public IEnumerable<ContentNodeKit> GetBranchContentSources(int id)
        => _repository.GetBranchContentSources(id);

    /// <inheritdoc />
    public IEnumerable<ContentNodeKit> GetBranchMediaSources(int id)
        => _repository.GetBranchMediaSources(id);

    /// <inheritdoc />
    public ContentNodeKit GetContentSource(int id)
        => _repository.GetContentSource(id);

    /// <inheritdoc />
    public ContentNodeKit GetMediaSource(int id)
        => _repository.GetMediaSource(id);

    /// <inheritdoc />
    public IEnumerable<ContentNodeKit> GetTypeContentSources(IEnumerable<int>? ids)
        => _repository.GetTypeContentSources(ids);

    /// <inheritdoc />
    public IEnumerable<ContentNodeKit> GetTypeMediaSources(IEnumerable<int> ids)
        => _repository.GetTypeContentSources(ids);

    /// <inheritdoc />
    public void DeleteContentItem(IContentBase item)
        => _repository.DeleteContentItem(item);

    public void DeleteContentItems(IEnumerable<IContentBase> items)
    {
        foreach (IContentBase item in items)
        {
            _repository.DeleteContentItem(item);
        }
    }

    /// <inheritdoc />
    public void RefreshContent(IContent content)
        => _repository.RefreshContent(content);

    /// <inheritdoc />
    public void RefreshMedia(IMedia media)
        => _repository.RefreshMedia(media);

    /// <inheritdoc />
    public void RefreshMember(IMember member)
        => _repository.RefreshMember(member);

    /// <inheritdoc />
    public void RebuildAll()
    {
        Rebuild(Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>());
    }

    /// <inheritdoc />
    public void Rebuild(
    IReadOnlyCollection<int>? contentTypeIds = null,
    IReadOnlyCollection<int>? mediaTypeIds = null,
    IReadOnlyCollection<int>? memberTypeIds = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            scope.ReadLock(Constants.Locks.MediaTree);
            scope.ReadLock(Constants.Locks.MemberTree);

            _repository.Rebuild(contentTypeIds, mediaTypeIds, memberTypeIds);

            // Save a key/value of the serialized type. This is used during startup to see
            // if the serialized type changed and if so it will rebuild with the correct type.
            _keyValueService.SetValue(NuCacheSerializerKey, _nucacheSettings.Value.NuCacheSerializerType.ToString());

            scope.Complete();
        }
    }

    /// <inheritdoc />
    public bool VerifyContentDbCache()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return _repository.VerifyContentDbCache();
    }

    /// <inheritdoc />
    public bool VerifyMediaDbCache()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.MediaTree);
        return _repository.VerifyMediaDbCache();
    }

    /// <inheritdoc />
    public bool VerifyMemberDbCache()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.MemberTree);
        return _repository.VerifyMemberDbCache();
    }
}
