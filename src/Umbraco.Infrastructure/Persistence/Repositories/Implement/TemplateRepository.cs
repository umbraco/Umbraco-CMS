using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Cache;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents the template repository.
/// </summary>
internal sealed class TemplateRepository : AsyncEntityRepositoryBase<Guid, ITemplate>, ITemplateRepository
{
    private const int MaxAliasLength = 100;
    private const int TruncatedAliasLength = 95;

    private readonly IShortStringHelper _shortStringHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateRepository" /> class.
    /// </summary>
    public TemplateRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches cache,
        ILogger<TemplateRepository> logger,
        IShortStringHelper shortStringHelper,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
        => _shortStringHelper = shortStringHelper;

    /// <inheritdoc />
    public async Task<ITemplate?> GetByAliasAsync(string alias, CancellationToken cancellationToken)
    {
        IEnumerable<ITemplate> all = await GetAllAsync(cancellationToken);
        return all.FirstOrDefault(template => template.Alias.InvariantEquals(alias));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ITemplate>> GetDescendantsAsync(Guid? layoutTemplateKey, CancellationToken cancellationToken)
    {
        ITemplate[] all = (await GetAllAsync(cancellationToken)).ToArray();
        var descendants = new List<ITemplate>();

        if (layoutTemplateKey.HasValue)
        {
            ITemplate? layoutTemplate = all.FirstOrDefault(template => template.Key == layoutTemplateKey.Value);
            if (layoutTemplate is null)
            {
                return [];
            }

            AddDescendants(all, descendants, layoutTemplate.Alias);
            return descendants;
        }

        ITemplate[] rootTemplates = all.Where(template => template.LayoutTemplateAlias.IsNullOrWhiteSpace()).ToArray();
        descendants.AddRange(rootTemplates);
        foreach (ITemplate rootTemplate in rootTemplates)
        {
            AddDescendants(all, descendants, rootTemplate.Alias);
        }

        return descendants;
    }

    /// <inheritdoc />
    protected override IAsyncRepositoryCachePolicy<ITemplate, Guid> CreateCachePolicy()
        => new AsyncFullDataSetRepositoryCachePolicy<ITemplate, Guid>(
            GlobalIsolatedCache,
            ScopeAccessor,
            RepositoryCacheVersionService,
            CacheSyncService,
            GetEntityKey,
            false);

    /// <inheritdoc />
    protected override async Task<ITemplate?> PerformGetAsync(Guid key)
    {
        IEnumerable<ITemplate> all = await GetAllAsync(CancellationToken.None);
        return all.FirstOrDefault(template => template.Key == key);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<ITemplate>?> PerformGetAllAsync() =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<TemplateDto> dtos = await db.Templates
                .Include(template => template.NodeDto)
                .Where(template => template.NodeDto.NodeObjectType == Constants.ObjectTypes.Template)
                .OrderBy(template => template.NodeId)
                .ToListAsync();

            return BuildEntities(dtos);
        });

    /// <inheritdoc />
    protected override async Task<IEnumerable<ITemplate>?> PerformGetManyAsync(Guid[] keys)
    {
        IEnumerable<ITemplate> all = await GetAllAsync(CancellationToken.None);
        if (keys.Length == 0)
        {
            return all;
        }

        var keySet = keys.ToHashSet();
        return all.Where(template => keySet.Contains(template.Key));
    }

    /// <inheritdoc />
    protected override Task<bool> PerformExistsAsync(Guid key) =>
        AmbientScope.ExecuteWithContextAsync(db => db.Nodes
            .AnyAsync(node => node.UniqueId == key && node.NodeObjectType == Constants.ObjectTypes.Template));

    /// <inheritdoc />
    protected override async Task PersistNewItemAsync(ITemplate entity) =>
        await AmbientScope.ExecuteWithContextAsync<TemplateDto>(async db =>
        {
            await EnsureValidAliasAsync(db, entity);

            entity.AddingEntity();

            NodeDto nodeDto = TemplateFactory.BuildNodeDto(entity);
            var parentPath = await GetParentPathAsync(db, nodeDto.ParentId);

            // The node id is only known once inserted, so the path is completed afterwards on the tracked node.
            nodeDto.Path = parentPath;
            db.Nodes.Add(nodeDto);
            await db.SaveChangesAsync();

            nodeDto.Path = string.Concat(parentPath, ",", nodeDto.NodeId);
            await db.SaveChangesAsync();

            db.Templates.Add(new TemplateDto { NodeId = nodeDto.NodeId, Alias = entity.Alias });
            await db.SaveChangesAsync();

            entity.Id = nodeDto.NodeId;
            entity.Path = nodeDto.Path;
            entity.ResetDirtyProperties();
        });

    /// <inheritdoc />
    protected override async Task PersistUpdatedItemAsync(ITemplate entity) =>
        await AmbientScope.ExecuteWithContextAsync<TemplateDto>(async db =>
        {
            await EnsureValidAliasAsync(db, entity);

            entity.UpdatingEntity();

            NodeDto nodeDto = TemplateFactory.BuildNodeDto(entity);

            if (entity.IsPropertyDirty(nameof(Template.LayoutTemplateId)))
            {
                var parentPath = await GetParentPathAsync(db, nodeDto.ParentId);
                entity.Path = string.Concat(parentPath, ",", entity.Id);
                nodeDto.Path = entity.Path;
            }

            await db.Nodes
                .Where(node => node.NodeId == entity.Id)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(node => node.ParentId, nodeDto.ParentId)
                    .SetProperty(node => node.Path, nodeDto.Path)
                    .SetProperty(node => node.Text, nodeDto.Text));

            await db.Templates
                .Where(template => template.NodeId == entity.Id)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(template => template.Alias, entity.Alias));

            entity.IsLayoutTemplate = await db.Nodes
                .AnyAsync(node => node.ParentId == entity.Id && node.NodeObjectType == Constants.ObjectTypes.Template);

            entity.ResetDirtyProperties();
        });

    /// <inheritdoc />
    protected override async Task PersistDeletedItemAsync(ITemplate entity)
    {
        IEnumerable<ITemplate> descendants = await GetDescendantsAsync(entity.Key, CancellationToken.None);
        var nodeIds = descendants.Select(descendant => descendant.Id).Append(entity.Id).ToList();

        await AmbientScope.ExecuteWithContextAsync<TemplateDto>(async db =>
        {
            await db.User2NodeNotifies
                .Where(notify => nodeIds.Contains(notify.NodeId))
                .ExecuteDeleteAsync();

            await db.DocumentVersions
                .Where(documentVersion => documentVersion.TemplateId.HasValue && nodeIds.Contains(documentVersion.TemplateId.Value))
                .ExecuteUpdateAsync(setter => setter.SetProperty(documentVersion => documentVersion.TemplateId, (int?)null));

            await db.ContentTypeTemplates
                .Where(contentTypeTemplate => nodeIds.Contains(contentTypeTemplate.TemplateNodeId))
                .ExecuteDeleteAsync();

            await db.Templates
                .Where(template => nodeIds.Contains(template.NodeId))
                .ExecuteDeleteAsync();

            await db.Nodes
                .Where(node => nodeIds.Contains(node.NodeId))
                .ExecuteDeleteAsync();
        });

        entity.DeleteDate = DateTime.UtcNow;
    }

    private IEnumerable<ITemplate> BuildEntities(List<TemplateDto> dtos)
    {
        Dictionary<int, string?> aliasesById = dtos.ToDictionary(dto => dto.NodeId, dto => dto.Alias);
        var layoutTemplateIds = dtos.Select(dto => dto.NodeDto.ParentId).ToHashSet();

        return dtos
            .Select(dto => TemplateFactory.BuildEntity(
                _shortStringHelper,
                dto,
                aliasesById.GetValueOrDefault(dto.NodeDto.ParentId),
                layoutTemplateIds.Contains(dto.NodeId)))
            .ToList();
    }

    private static async Task<string> GetParentPathAsync(UmbracoDbContext db, int parentId)
    {
        if (parentId <= 0)
        {
            return Constants.System.RootString;
        }

        return await db.Nodes
            .Where(node => node.NodeId == parentId && node.NodeObjectType == Constants.ObjectTypes.Template)
            .Select(node => node.Path)
            .FirstOrDefaultAsync()
            ?? Constants.System.RootString;
    }

    /// <summary>
    ///     Cleans and truncates the template alias, and makes it unique by appending a number if another template
    ///     already uses it.
    /// </summary>
    private async Task EnsureValidAliasAsync(UmbracoDbContext db, ITemplate template)
    {
        var alias = template.Alias.ToCleanString(_shortStringHelper, CleanStringType.UnderscoreAlias);
        if (alias.Length > MaxAliasLength)
        {
            alias = alias[..TruncatedAliasLength];
        }

        var templateId = template.Id;
        List<string?> similarAliases = await db.Templates
            .Where(existing => existing.NodeId != templateId && existing.Alias != null && existing.Alias.StartsWith(alias))
            .Select(existing => existing.Alias)
            .ToListAsync();

        var takenAliases = new HashSet<string>(similarAliases.WhereNotNull(), StringComparer.InvariantCultureIgnoreCase);
        if (takenAliases.Contains(alias))
        {
            var suffix = 1;
            while (takenAliases.Contains(alias + suffix))
            {
                suffix++;
            }

            alias += suffix;
        }

        template.Alias = alias;
    }

    private static void AddDescendants(ITemplate[] all, List<ITemplate> descendants, string layoutTemplateAlias)
    {
        ITemplate[] children = all
            .Where(template => template.LayoutTemplateAlias.InvariantEquals(layoutTemplateAlias))
            .ToArray();

        descendants.AddRange(children);
        foreach (ITemplate child in children)
        {
            AddDescendants(all, descendants, child.Alias);
        }
    }
}
