using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents the Template Repository
/// </summary>
internal class TemplateRepository : EntityRepositoryBase<int, ITemplate>, ITemplateRepository
{
    private readonly IIOHelper _ioHelper;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IFileSystem? _viewsFileSystem;
    private readonly IViewHelper _viewHelper;
    private readonly IOptionsMonitor<RuntimeSettings> _runtimeSettings;

    public TemplateRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<TemplateRepository> logger,
        FileSystems fileSystems,
        IIOHelper ioHelper,
        IShortStringHelper shortStringHelper,
        IViewHelper viewHelper,
        IOptionsMonitor<RuntimeSettings> runtimeSettings)
        : base(scopeAccessor, cache, logger)
    {
        _ioHelper = ioHelper;
        _shortStringHelper = shortStringHelper;
        _viewsFileSystem = fileSystems.MvcViewsFileSystem;
        _viewHelper = viewHelper;
        _runtimeSettings = runtimeSettings;
    }

    public Stream GetFileContentStream(string filepath)
    {
        IFileSystem? fileSystem = GetFileSystem(filepath);
        if (fileSystem?.FileExists(filepath) == false)
        {
            return Stream.Null;
        }

        try
        {
            return fileSystem!.OpenFile(filepath);
        }
        catch
        {
            return Stream.Null; // deal with race conds
        }
    }

    public void SetFileContent(string filepath, Stream content) =>
        GetFileSystem(filepath)?.AddFile(filepath, content, true);

    public long GetFileSize(string filename)
    {
        IFileSystem? fileSystem = GetFileSystem(filename);
        if (fileSystem?.FileExists(filename) == false)
        {
            return -1;
        }

        try
        {
            return fileSystem!.GetSize(filename);
        }
        catch
        {
            return -1; // deal with race conds
        }
    }

    protected override IRepositoryCachePolicy<ITemplate, int> CreateCachePolicy() =>
        new FullDataSetRepositoryCachePolicy<ITemplate, int>(GlobalIsolatedCache, ScopeAccessor,
            GetEntityId, /*expires:*/ false);

    private IEnumerable<IUmbracoEntity> GetAxisDefinitions(params TemplateDto[] templates)
    {
        //look up the simple template definitions that have a master template assigned, this is used
        // later to populate the template item's properties
        Sql<ISqlContext> childIdsSql = SqlContext.Sql()
            .Select("nodeId,alias,parentID")
            .From<TemplateDto>()
            .InnerJoin<NodeDto>()
            .On<TemplateDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
            //lookup axis's
            .Where(
                "umbracoNode." + SqlContext.SqlSyntax.GetQuotedColumnName("id") +
                " IN (@parentIds) OR umbracoNode.parentID IN (@childIds)",
                new
                {
                    parentIds = templates.Select(x => x.NodeDto.ParentId),
                    childIds = templates.Select(x => x.NodeId)
                });

        IEnumerable<EntitySlim> childIds = Database.Fetch<AxisDefintionDto>(childIdsSql)
            .Select(x => new EntitySlim {Id = x.NodeId, ParentId = x.ParentId, Name = x.Alias});

        return childIds;
    }

    /// <summary>
    ///     Maps from a dto to an ITemplate
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="axisDefinitions">
    ///     This is a collection of template definitions ... either all templates, or the collection of child templates and
    ///     it's parent template
    /// </param>
    /// <returns></returns>
    private ITemplate MapFromDto(TemplateDto dto, IUmbracoEntity[] axisDefinitions)
    {
        Template template = TemplateFactory.BuildEntity(_shortStringHelper, dto, axisDefinitions,
            file => GetFileContent((Template)file, false));

        if (dto.NodeDto.ParentId > 0)
        {
            IUmbracoEntity? masterTemplate = axisDefinitions.FirstOrDefault(x => x.Id == dto.NodeDto.ParentId);
            if (masterTemplate != null)
            {
                template.MasterTemplateAlias = masterTemplate.Name;
                template.MasterTemplateId = new Lazy<int>(() => dto.NodeDto.ParentId);
            }
        }

        // get the infos (update date and virtual path) that will change only if
        // path changes - but do not get content, will get loaded only when required
        GetFileContent(template, true);

        // reset dirty initial properties (U4-1946)
        template.ResetDirtyProperties(false);

        return template;
    }

    private void SetVirtualPath(ITemplate template)
    {
        var path = template.OriginalPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            // we need to discover the path
            path = string.Concat(template.Alias, ".cshtml");
            if (_viewsFileSystem?.FileExists(path) ?? false)
            {
                template.VirtualPath = _viewsFileSystem.GetUrl(path);
                return;
            }

            path = string.Concat(template.Alias, ".vbhtml");
            if (_viewsFileSystem?.FileExists(path) ?? false)
            {
                template.VirtualPath = _viewsFileSystem.GetUrl(path);
                return;
            }
        }
        else
        {
            // we know the path already
            template.VirtualPath = _viewsFileSystem?.GetUrl(path);
        }

        template.VirtualPath = string.Empty; // file not found...
    }

    private string? GetFileContent(ITemplate template, bool init)
    {
        var path = template.OriginalPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            // we need to discover the path
            path = string.Concat(template.Alias, ".cshtml");
            if (_viewsFileSystem?.FileExists(path) ?? false)
            {
                return GetFileContent(template, _viewsFileSystem, path, init);
            }

            path = string.Concat(template.Alias, ".vbhtml");
            if (_viewsFileSystem?.FileExists(path) ?? false)
            {
                return GetFileContent(template, _viewsFileSystem, path, init);
            }
        }
        else
        {
            // we know the path already
            return GetFileContent(template, _viewsFileSystem, path, init);
        }

        template.VirtualPath = string.Empty; // file not found...

        return string.Empty;
    }

    private string? GetFileContent(ITemplate template, IFileSystem? fs, string filename, bool init)
    {
        // do not update .UpdateDate as that would make it dirty (side-effect)
        // unless initializing, because we have to do it once
        if (init && fs is not null)
        {
            template.UpdateDate = fs.GetLastModified(filename).UtcDateTime;
        }

        // TODO: see if this could enable us to update UpdateDate without messing with change tracking
        // and then we'd want to do it for scripts, stylesheets and partial views too (ie files)
        // var xtemplate = template as Template;
        // xtemplate.DisableChangeTracking();
        // template.UpdateDate = fs.GetLastModified(filename).UtcDateTime;
        // xtemplate.EnableChangeTracking();

        template.VirtualPath = fs?.GetUrl(filename);

        return init ? null : GetFileContent(fs, filename);
    }

    private string? GetFileContent(IFileSystem? fs, string filename)
    {
        if (fs is null)
        {
            return null;
        }

        using Stream stream = fs.OpenFile(filename);
        using var reader = new StreamReader(stream, Encoding.UTF8, true);
        return reader.ReadToEnd();
    }

    private IFileSystem? GetFileSystem(string filepath)
    {
        var ext = Path.GetExtension(filepath);
        IFileSystem? fs;
        switch (ext)
        {
            case ".cshtml":
            case ".vbhtml":
                fs = _viewsFileSystem;
                break;
            default:
                throw new Exception("Unsupported extension " + ext + ".");
        }

        return fs;
    }

    /// <summary>
    ///     Ensures that there are not duplicate aliases and if so, changes it to be a numbered version and also verifies the
    ///     length
    /// </summary>
    /// <param name="template"></param>
    private void EnsureValidAlias(ITemplate template)
    {
        //ensure unique alias
        template.Alias = template.Alias.ToCleanString(_shortStringHelper, CleanStringType.UnderscoreAlias);

        if (template.Alias.Length > 100)
        {
            template.Alias = template.Alias.Substring(0, 95);
        }

        if (AliasAlreadExists(template))
        {
            template.Alias = EnsureUniqueAlias(template, 1);
        }
    }

    private bool AliasAlreadExists(ITemplate template)
    {
        Sql<ISqlContext> sql = GetBaseQuery(true)
            .Where<TemplateDto>(x => x.Alias.InvariantEquals(template.Alias) && x.NodeId != template.Id);
        var count = Database.ExecuteScalar<int>(sql);
        return count > 0;
    }

    private string EnsureUniqueAlias(ITemplate template, int attempts)
    {
        // TODO: This is ported from the old data layer... pretty crap way of doing this but it works for now.
        if (AliasAlreadExists(template))
        {
            return template.Alias + attempts;
        }

        attempts++;
        return EnsureUniqueAlias(template, attempts);
    }

    #region Overrides of RepositoryBase<int,ITemplate>

    protected override ITemplate? PerformGet(int id) =>
        //use the underlying GetAll which will force cache all templates
        GetMany().FirstOrDefault(x => x.Id == id);

    protected override IEnumerable<ITemplate> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);

        if (ids?.Any() ?? false)
        {
            sql.Where("umbracoNode.id in (@ids)", new {ids});
        }
        else
        {
            sql.Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
        }

        List<TemplateDto> dtos = Database.Fetch<TemplateDto>(sql);

        if (dtos.Count == 0)
        {
            return Enumerable.Empty<ITemplate>();
        }

        //look up the simple template definitions that have a master template assigned, this is used
        // later to populate the template item's properties
        IUmbracoEntity[] childIds = (ids?.Any() ?? false
                ? GetAxisDefinitions(dtos.ToArray())
                : dtos.Select(x => new EntitySlim {Id = x.NodeId, ParentId = x.NodeDto.ParentId, Name = x.Alias}))
            .ToArray();

        return dtos.Select(d => MapFromDto(d, childIds));
    }

    protected override IEnumerable<ITemplate> PerformGetByQuery(IQuery<ITemplate> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<ITemplate>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        List<TemplateDto> dtos = Database.Fetch<TemplateDto>(sql);

        if (dtos.Count == 0)
        {
            return Enumerable.Empty<ITemplate>();
        }

        //look up the simple template definitions that have a master template assigned, this is used
        // later to populate the template item's properties
        IUmbracoEntity[] childIds = GetAxisDefinitions(dtos.ToArray()).ToArray();

        return dtos.Select(d => MapFromDto(d, childIds));
    }

    #endregion

    #region Overrides of EntityRepositoryBase<int,ITemplate>

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = SqlContext.Sql();

        sql = isCount
            ? sql.SelectCount()
            : sql.Select<TemplateDto>(r => r.Select(x => x.NodeDto));

        sql
            .From<TemplateDto>()
            .InnerJoin<NodeDto>()
            .On<TemplateDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

        return sql;
    }

    protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.Node}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            "DELETE FROM " + Constants.DatabaseSchema.Tables.User2NodeNotify + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.UserGroup2Node + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.UserGroup2NodePermission + " WHERE nodeId = @id",
            "UPDATE " + Constants.DatabaseSchema.Tables.DocumentVersion +
            " SET templateId = NULL WHERE templateId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.DocumentType + " WHERE templateNodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.Template + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.Node + " WHERE id = @id"
        };
        return list;
    }

    protected Guid NodeObjectTypeId => Constants.ObjectTypes.Template;

    protected override void PersistNewItem(ITemplate entity)
    {
        EnsureValidAlias(entity);

        //Save to db
        var template = (Template)entity;
        template.AddingEntity();

        TemplateDto dto = TemplateFactory.BuildDto(template, NodeObjectTypeId, template.Id);

        //Create the (base) node data - umbracoNode
        NodeDto nodeDto = dto.NodeDto;
        nodeDto.Path = "-1," + dto.NodeDto.NodeId;
        var o = Database.IsNew(nodeDto) ? Convert.ToInt32(Database.Insert(nodeDto)) : Database.Update(nodeDto);

        //Update with new correct path
        ITemplate? parent = Get(template.MasterTemplateId!.Value);
        if (parent != null)
        {
            nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
        }
        else
        {
            nodeDto.Path = "-1," + dto.NodeDto.NodeId;
        }

        Database.Update(nodeDto);

        //Insert template dto
        dto.NodeId = nodeDto.NodeId;
        Database.Insert(dto);

        //Update entity with correct values
        template.Id = nodeDto.NodeId; //Set Id on entity to ensure an Id is set
        template.Path = nodeDto.Path;

        // Only save file when not in production runtime mode
        if (_runtimeSettings.CurrentValue.Mode != RuntimeMode.Production)
        {
            //now do the file work
            SaveFile(template);
        }

        template.ResetDirtyProperties();

        // ensure that from now on, content is lazy-loaded
        if (template.GetFileContent == null)
        {
            template.GetFileContent = file => GetFileContent((Template)file, false);
        }
    }

    protected override void PersistUpdatedItem(ITemplate entity)
    {
        EnsureValidAlias(entity);

        //store the changed alias if there is one for use with updating files later
        var originalAlias = entity.Alias;
        if (entity.IsPropertyDirty("Alias"))
        {
            //we need to check what it currently is before saving and remove that file
            ITemplate? current = Get(entity.Id);
            originalAlias = current?.Alias;
        }

        var template = (Template)entity;

        if (entity.IsPropertyDirty("MasterTemplateId"))
        {
            ITemplate? parent = Get(template.MasterTemplateId!.Value);
            if (parent != null)
            {
                entity.Path = string.Concat(parent.Path, ",", entity.Id);
            }
            else
            {
                //this means that the master template has been removed, so we need to reset the template's
                //path to be at the root
                entity.Path = string.Concat("-1,", entity.Id);
            }
        }

        //Get TemplateDto from db to get the Primary key of the entity
        TemplateDto templateDto = Database.SingleOrDefault<TemplateDto>("WHERE nodeId = @Id", new {entity.Id});

        //Save updated entity to db
        template.UpdateDate = DateTime.Now;
        TemplateDto dto = TemplateFactory.BuildDto(template, NodeObjectTypeId, templateDto.PrimaryKey);
        Database.Update(dto.NodeDto);
        Database.Update(dto);

        //re-update if this is a master template, since it could have changed!
        IEnumerable<IUmbracoEntity> axisDefs = GetAxisDefinitions(dto);
        template.IsMasterTemplate = axisDefs.Any(x => x.ParentId == dto.NodeId);

        // Only save file when not in production runtime mode
        if (_runtimeSettings.CurrentValue.Mode != RuntimeMode.Production)
        {
            //now do the file work
            SaveFile((Template)entity, originalAlias);
        }

        entity.ResetDirtyProperties();

        // ensure that from now on, content is lazy-loaded
        if (template.GetFileContent == null)
        {
            template.GetFileContent = file => GetFileContent((Template)file, false);
        }
    }

    private void SaveFile(Template template, string? originalAlias = null)
    {
        string? content;

        if (template is TemplateOnDisk templateOnDisk && templateOnDisk.IsOnDisk)
        {
            // if "template on disk" load content from disk
            content = _viewHelper.GetFileContents(template);
        }
        else
        {
            // else, create or write template.Content to disk
            content = originalAlias == null
                ? _viewHelper.CreateView(template, true)
                : _viewHelper.UpdateViewFile(template, originalAlias);
        }

        // once content has been set, "template on disk" are not "on disk" anymore
        template.Content = content;
        SetVirtualPath(template);
    }

    protected override void PersistDeletedItem(ITemplate entity)
    {
        var deletes = GetDeleteClauses().ToArray();

        var descendants = GetDescendants(entity.Id).ToList();

        //change the order so it goes bottom up! (deepest level first)
        descendants.Reverse();

        //delete the hierarchy
        foreach (ITemplate descendant in descendants)
        {
            foreach (var delete in deletes)
            {
                Database.Execute(delete, new {id = GetEntityId(descendant)});
            }
        }

        //now we can delete this one
        foreach (var delete in deletes)
        {
            Database.Execute(delete, new {id = GetEntityId(entity)});
        }

        var viewName = string.Concat(entity.Alias, ".cshtml");
        _viewsFileSystem?.DeleteFile(viewName);

        entity.DeleteDate = DateTime.Now;
    }

    #endregion

    #region Implementation of ITemplateRepository

    public ITemplate? Get(string? alias) => GetAll(alias).FirstOrDefault();

    public IEnumerable<ITemplate> GetAll(params string?[] aliases)
    {
        //We must call the base (normal) GetAll method
        // which is cached. This is a specialized method and unfortunately with the params[] it
        // overlaps with the normal GetAll method.
        if (aliases.Any() == false)
        {
            return GetMany();
        }

        //return from base.GetAll, this is all cached
        return GetMany().Where(x => aliases.WhereNotNull().InvariantContains(x.Alias));
    }

    public IEnumerable<ITemplate> GetChildren(int masterTemplateId)
    {
        //return from base.GetAll, this is all cached
        ITemplate[] all = GetMany().ToArray();

        if (masterTemplateId <= 0)
        {
            return all.Where(x => x.MasterTemplateAlias.IsNullOrWhiteSpace());
        }

        ITemplate? parent = all.FirstOrDefault(x => x.Id == masterTemplateId);
        if (parent == null)
        {
            return Enumerable.Empty<ITemplate>();
        }

        IEnumerable<ITemplate> children = all.Where(x => x.MasterTemplateAlias.InvariantEquals(parent.Alias));
        return children;
    }

    public IEnumerable<ITemplate> GetDescendants(int masterTemplateId)
    {
        //return from base.GetAll, this is all cached
        ITemplate[] all = GetMany().ToArray();
        var descendants = new List<ITemplate>();
        if (masterTemplateId > 0)
        {
            ITemplate? parent = all.FirstOrDefault(x => x.Id == masterTemplateId);
            if (parent == null)
            {
                return Enumerable.Empty<ITemplate>();
            }

            //recursively add all children with a level
            AddChildren(all, descendants, parent.Alias);
        }
        else
        {
            descendants.AddRange(all.Where(x => x.MasterTemplateAlias.IsNullOrWhiteSpace()));
            foreach (ITemplate parent in descendants)
            {
                //recursively add all children with a level
                AddChildren(all, descendants, parent.Alias);
            }
        }

        //return the list - it will be naturally ordered by level
        return descendants;
    }

    private void AddChildren(ITemplate[]? all, List<ITemplate> descendants, string masterAlias)
    {
        ITemplate[]? c = all?.Where(x => x.MasterTemplateAlias.InvariantEquals(masterAlias)).ToArray();
        if (c is null || c.Any() == false)
        {
            return;
        }

        descendants.AddRange(c);

        //recurse through all children
        foreach (ITemplate child in c)
        {
            AddChildren(all, descendants, child.Alias);
        }
    }

    #endregion
}
