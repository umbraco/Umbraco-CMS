using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the Template Repository
    /// </summary>
    internal class TemplateRepository : PetaPocoRepositoryBase<int, ITemplate>, ITemplateRepository
    {
        private readonly IFileSystem _masterpagesFileSystem;
        private readonly IFileSystem _viewsFileSystem;
        private readonly ITemplatesSection _templateConfig;
        private readonly ViewHelper _viewHelper;
        private readonly MasterPageHelper _masterPageHelper;
        private readonly RepositoryCacheOptions _cacheOptions;

        internal TemplateRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax, IFileSystem masterpageFileSystem, IFileSystem viewFileSystem, ITemplatesSection templateConfig)
            : base(work, cache, logger, sqlSyntax)
        {
            _masterpagesFileSystem = masterpageFileSystem;
            _viewsFileSystem = viewFileSystem;
            _templateConfig = templateConfig;
            _viewHelper = new ViewHelper(_viewsFileSystem);
            _masterPageHelper = new MasterPageHelper(_masterpagesFileSystem);

            _cacheOptions = new RepositoryCacheOptions
            {
                //Allow a zero count cache entry because GetAll() gets used quite a lot and we want to ensure
                // if there are no templates, that it doesn't keep going to the db.
                GetAllCacheAllowZeroCount = true,
                //GetAll gets called a lot, we want to ensure that all templates are in the cache, default is 100 which
                // would normally be fine but we'll increase it in case people have a ton of templates.
                GetAllCacheThresholdLimit = 500
            };
        }


        /// <summary>
        /// Returns the repository cache options
        /// </summary>
        protected override RepositoryCacheOptions RepositoryCacheOptions
        {
            get { return _cacheOptions; }
        }

        #region Overrides of RepositoryBase<int,ITemplate>

        protected override ITemplate PerformGet(int id)
        {
            var sql = GetBaseQuery(false).Where<TemplateDto>(x => x.NodeId == id);
            var result = Database.Fetch<TemplateDto, NodeDto>(sql).FirstOrDefault();
            if (result == null) return null;

            //look up the simple template definitions that have a master template assigned, this is used 
            // later to populate the template item's properties
            var childIds = GetAxisDefinitions(result).ToArray();

            return MapFromDto(result, childIds);
        }

        protected override IEnumerable<ITemplate> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);

            if (ids.Any())
            {
                sql.Where("umbracoNode.id in (@ids)", new { ids = ids });
            }
            else
            {
                sql.Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            }

            var dtos = Database.Fetch<TemplateDto, NodeDto>(sql);

            if (dtos.Count == 0) return Enumerable.Empty<ITemplate>();

            //look up the simple template definitions that have a master template assigned, this is used 
            // later to populate the template item's properties
            var childIds = (ids.Any()
                ? GetAxisDefinitions(dtos.ToArray())
                : dtos.Select(x => new UmbracoEntity
                {
                    Id = x.NodeId,
                    ParentId = x.NodeDto.ParentId,
                    Name = x.Alias
                })).ToArray();
            
            return dtos.Select(d => MapFromDto(d, childIds));
        }

        protected override IEnumerable<ITemplate> PerformGetByQuery(IQuery<ITemplate> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<ITemplate>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<TemplateDto, NodeDto>(sql);

            if (dtos.Count == 0) return Enumerable.Empty<ITemplate>();

            //look up the simple template definitions that have a master template assigned, this is used 
            // later to populate the template item's properties
            var childIds = GetAxisDefinitions(dtos.ToArray()).ToArray();

            return dtos.Select(d => MapFromDto(d, childIds));
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,ITemplate>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
                .From<TemplateDto>(SqlSyntax)
                .InnerJoin<NodeDto>(SqlSyntax)
                .On<TemplateDto, NodeDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @Id",
                               "DELETE FROM umbracoUser2NodePermission WHERE nodeId = @Id",
                               "UPDATE cmsDocument SET templateId = NULL WHERE templateId = @Id",
                               "DELETE FROM cmsDocumentType WHERE templateNodeId = @Id",
                               "DELETE FROM cmsTemplate WHERE nodeId = @Id",
                               "DELETE FROM umbracoNode WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.Template); }
        }

        protected override void PersistNewItem(ITemplate entity)
        {
            EnsureValidAlias(entity);

            //Save to db
            var template = (Template)entity;
            template.AddingEntity();

            var factory = new TemplateFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(template);

            //Create the (base) node data - umbracoNode
            var nodeDto = dto.NodeDto;
            nodeDto.Path = "-1," + dto.NodeDto.NodeId;
            var o = Database.IsNew(nodeDto) ? Convert.ToInt32(Database.Insert(nodeDto)) : Database.Update(nodeDto);

            //Update with new correct path
            var parent = Get(template.MasterTemplateId.Value);
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

            //now do the file work

            if (DetermineTemplateRenderingEngine(entity) == RenderingEngine.Mvc)
            {
                var result = _viewHelper.CreateView(template, true);
                if (result != entity.Content)
                {
                    entity.Content = result;
                    //re-persist it... though we don't really care about the templates in the db do we??!!
                    dto.Design = result;
                    Database.Update(dto);
                }
            }
            else
            {
                var result = _masterPageHelper.CreateMasterPage(template, this, true);
                if (result != entity.Content)
                {
                    entity.Content = result;
                    //re-persist it... though we don't really care about the templates in the db do we??!!
                    dto.Design = result;
                    Database.Update(dto);
                }
            }

            template.ResetDirtyProperties();

            // ensure that from now on, content is lazy-loaded
            if (template.GetFileContent == null)
                template.GetFileContent = file => GetFileContent((Template) file, false);
        }

        protected override void PersistUpdatedItem(ITemplate entity)
        {
            EnsureValidAlias(entity);

            //store the changed alias if there is one for use with updating files later
            var originalAlias = entity.Alias;
            if (entity.IsPropertyDirty("Alias"))
            {
                //we need to check what it currently is before saving and remove that file
                var current = Get(entity.Id);
                originalAlias = current.Alias;
            }

            var template = (Template)entity;

            if (entity.IsPropertyDirty("MasterTemplateId"))
            {
                var parent = Get(template.MasterTemplateId.Value);
                if (parent != null)
                {
                    entity.Path = string.Concat(parent.Path, ",", entity.Id);
                }

            }

            //Get TemplateDto from db to get the Primary key of the entity
            var templateDto = Database.SingleOrDefault<TemplateDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            //Save updated entity to db

            template.UpdateDate = DateTime.Now;
            var factory = new TemplateFactory(templateDto.PrimaryKey, NodeObjectTypeId);
            var dto = factory.BuildDto(template);

            Database.Update(dto.NodeDto);
            Database.Update(dto);

            //re-update if this is a master template, since it could have changed!
            var axisDefs = GetAxisDefinitions(dto);
            template.IsMasterTemplate = axisDefs.Any(x => x.ParentId == dto.NodeId);

            //now do the file work

            if (DetermineTemplateRenderingEngine(entity) == RenderingEngine.Mvc)
            {
                var result = _viewHelper.UpdateViewFile(entity, originalAlias);
                if (result != entity.Content)
                {
                    entity.Content = result;
                    //re-persist it... though we don't really care about the templates in the db do we??!!
                    dto.Design = result;
                    Database.Update(dto);
                }
            }
            else
            {
                var result = _masterPageHelper.UpdateMasterPageFile(entity, originalAlias, this);
                if (result != entity.Content)
                {
                    entity.Content = result;
                    //re-persist it... though we don't really care about the templates in the db do we??!!
                    dto.Design = result;
                    Database.Update(dto);
                }
            }

            entity.ResetDirtyProperties();

            // ensure that from now on, content is lazy-loaded
            if (template.GetFileContent == null)
                template.GetFileContent = file => GetFileContent((Template) file, false);
        }

        protected override void PersistDeletedItem(ITemplate entity)
        {
            var deletes = GetDeleteClauses().ToArray();

            var descendants = GetDescendants(entity.Id).ToList();

            //change the order so it goes bottom up! (deepest level first)
            descendants.Reverse();

            //delete the hierarchy
            foreach (var descendant in descendants)
            {
                foreach (var delete in deletes)
                {
                    Database.Execute(delete, new { Id = GetEntityId(descendant) });
                }
            }

            //now we can delete this one
            foreach (var delete in deletes)
            {
                Database.Execute(delete, new { Id = GetEntityId(entity) });
            }

            if (DetermineTemplateRenderingEngine(entity) == RenderingEngine.Mvc)
            {
                var viewName = string.Concat(entity.Alias, ".cshtml");
                _viewsFileSystem.DeleteFile(viewName);
            }
            else
            {
                var masterpageName = string.Concat(entity.Alias, ".master");
                _masterpagesFileSystem.DeleteFile(masterpageName);
            }         
        }

        #endregion

        private IEnumerable<IUmbracoEntity> GetAxisDefinitions(params TemplateDto[] templates)
        {
            //look up the simple template definitions that have a master template assigned, this is used 
            // later to populate the template item's properties
            var childIdsSql = new Sql()
                .Select("nodeId,alias,parentID")
                .From<TemplateDto>(SqlSyntax)
                .InnerJoin<NodeDto>(SqlSyntax)
                .On<TemplateDto, NodeDto>(SqlSyntax, dto => dto.NodeId, dto => dto.NodeId)
                //lookup axis's
                .Where("umbracoNode." + SqlSyntax.GetQuotedColumnName("id") + " IN (@parentIds) OR umbracoNode.parentID IN (@childIds)",
                    new {parentIds = templates.Select(x => x.NodeDto.ParentId), childIds = templates.Select(x => x.NodeId)});            

            var childIds = Database.Fetch<dynamic>(childIdsSql)
                .Select(x => new UmbracoEntity
                {
                    Id = x.nodeId,
                    ParentId = x.parentID,
                    Name = x.alias
                });
            return childIds;
        }

        /// <summary>
        /// Maps from a dto to an ITemplate
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="axisDefinitions">
        /// This is a collection of template definitions ... either all templates, or the collection of child templates and it's parent template
        /// </param>
        /// <returns></returns>
        private ITemplate MapFromDto(TemplateDto dto, IUmbracoEntity[] axisDefinitions)
        {
            var factory = new TemplateFactory();
            var template = factory.BuildEntity(dto, axisDefinitions, file => GetFileContent((Template) file, false));

            if (dto.NodeDto.ParentId > 0)
            {
                var masterTemplate = axisDefinitions.FirstOrDefault(x => x.Id == dto.NodeDto.ParentId);
                if (masterTemplate != null)
                {
                    template.MasterTemplateAlias = masterTemplate.Name;
                    template.MasterTemplateId = new Lazy<int>(() => dto.NodeDto.ParentId);
                }
            }

            // get the infos (update date and virtual path) that will change only if
            // path changes - but do not get content, will get loaded only when required
            GetFileContent(template, true);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            template.ResetDirtyProperties(false);

            return template;
        }

        private string GetFileContent(ITemplate template, bool init)
        {
            var path = template.OriginalPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                // we need to discover the path
                path = string.Concat(template.Alias, ".cshtml");
                if (_viewsFileSystem.FileExists(path))
                    return GetFileContent(template, _viewsFileSystem, path, init);
                path = string.Concat(template.Alias, ".vbhtml");
                if (_viewsFileSystem.FileExists(path))
                    return GetFileContent(template, _viewsFileSystem, path, init);
                path = string.Concat(template.Alias, ".master");
                if (_masterpagesFileSystem.FileExists(path))
                    return GetFileContent(template, _masterpagesFileSystem, path, init);
            }
            else
            {
                // we know the path already
                var ext = Path.GetExtension(path);
                switch (ext)
                {
                    case ".cshtml":
                    case ".vbhtml":
                        return GetFileContent(template, _viewsFileSystem, path, init);
                    case ".master":
                        return GetFileContent(template, _masterpagesFileSystem, path, init);
                    default:
                        return string.Empty;
                }
            }

            var fsname = string.Concat(template.Alias, ".cshtml");
            if (_viewsFileSystem.FileExists(fsname))
                return GetFileContent(template, _viewsFileSystem, fsname, init);
            fsname = string.Concat(template.Alias, ".vbhtml");
            if (_viewsFileSystem.FileExists(fsname))
                return GetFileContent(template, _viewsFileSystem, fsname, init);
            fsname = string.Concat(template.Alias, ".master");
            if (_masterpagesFileSystem.FileExists(fsname))
                return GetFileContent(template, _masterpagesFileSystem, fsname, init);
            return string.Empty;
        }

        private string GetFileContent(ITemplate template, IFileSystem fs, string filename, bool init)
        {
            // do not update .UpdateDate as that would make it dirty (side-effect)
            // unless initializing, because we have to do it once
            if (init)
            {
                template.UpdateDate = fs.GetLastModified(filename).UtcDateTime;
            }

            // TODO
            //  see if this could enable us to update UpdateDate without messing with change tracking
            //  and then we'd want to do it for scripts, stylesheets and partial views too (ie files)
            //var xtemplate = template as Template;
            //xtemplate.DisableChangeTracking();
            //template.UpdateDate = fs.GetLastModified(filename).UtcDateTime;
            //xtemplate.EnableChangeTracking();

            template.VirtualPath = fs.GetUrl(filename);

            return init ? null : GetFileContent(fs, filename);
        }

        private string GetFileContent(IFileSystem fs, string filename)
        {
            using (var stream = fs.OpenFile(filename))
            using (var reader = new StreamReader(stream, Encoding.UTF8, true))
            {
                return reader.ReadToEnd();
            }
        }

        #region Implementation of ITemplateRepository

        public ITemplate Get(string alias)
        {
            var sql = GetBaseQuery(false).Where<TemplateDto>(x => x.Alias == alias);

            var dto = Database.Fetch<TemplateDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            return MapFromDto(dto, GetAxisDefinitions(dto).ToArray());
        }

        public IEnumerable<ITemplate> GetAll(params string[] aliases)
        {
            var sql = GetBaseQuery(false);

            if (aliases.Any())
            {
                sql.Where("cmsTemplate.alias IN (@aliases)", new {aliases = aliases});
            }

            var dtos = Database.Fetch<TemplateDto, NodeDto>(sql).ToArray();
            if (dtos.Length == 0) return Enumerable.Empty<ITemplate>();

            var axisDefos = GetAxisDefinitions(dtos).ToArray();
            return dtos.Select(x => MapFromDto(x, axisDefos));
        }

        public IEnumerable<ITemplate> GetChildren(int masterTemplateId)
        {
            var sql = GetBaseQuery(false);         
            if (masterTemplateId <= 0)
            {
                sql.Where<NodeDto>(x => x.ParentId <= 0);
            }
            else
            {
                sql.Where<NodeDto>(x => x.ParentId == masterTemplateId);
            }

            var dtos = Database.Fetch<TemplateDto, NodeDto>(sql).ToArray();
            if (dtos.Length == 0) return Enumerable.Empty<ITemplate>();

            var axisDefos = GetAxisDefinitions(dtos).ToArray();
            return dtos.Select(x => MapFromDto(x, axisDefos));
        }

        public IEnumerable<ITemplate> GetChildren(string alias)
        {
            var sql = GetBaseQuery(false);
            if (alias.IsNullOrWhiteSpace())
            {
                sql.Where<NodeDto>(x => x.ParentId <= 0);
            }
            else
            {
                //unfortunately SQLCE doesn't support scalar subqueries in the where clause, otherwise we could have done this
                // in a single query, now we have to lookup the path to acheive the same thing
                var parent = Database.ExecuteScalar<int?>(new Sql().Select("nodeId").From<TemplateDto>(SqlSyntax).Where<TemplateDto>(dto => dto.Alias == alias));
                if (parent.HasValue == false) return Enumerable.Empty<ITemplate>();

                sql.Where<NodeDto>(x => x.ParentId == parent.Value);
            }

            var dtos = Database.Fetch<TemplateDto, NodeDto>(sql).ToArray();
            if (dtos.Length == 0) return Enumerable.Empty<ITemplate>();

            var axisDefos = GetAxisDefinitions(dtos).ToArray();
            return dtos.Select(x => MapFromDto(x, axisDefos));
        }

        public IEnumerable<ITemplate> GetDescendants(int masterTemplateId)
        {
            var sql = GetBaseQuery(false);
            if (masterTemplateId > 0)
            {
                //unfortunately SQLCE doesn't support scalar subqueries in the where clause, otherwise we could have done this
                // in a single query, now we have to lookup the path to acheive the same thing
                var path = Database.ExecuteScalar<string>(
                    new Sql().Select(SqlSyntax.GetQuotedColumnName("path"))
                        .From<TemplateDto>(SqlSyntax)
                        .InnerJoin<NodeDto>(SqlSyntax)
                        .On<TemplateDto, NodeDto>(SqlSyntax, dto => dto.NodeId, dto => dto.NodeId)
                        .Where<NodeDto>(dto => dto.NodeId == masterTemplateId));

                if (path.IsNullOrWhiteSpace()) return Enumerable.Empty<ITemplate>();

                sql.Where(@"(umbracoNode." + SqlSyntax.GetQuotedColumnName("path") + @" LIKE @query)", new { query = path + ",%" });
            }

            sql.OrderBy("umbracoNode." + SqlSyntax.GetQuotedColumnName("level"));

            var dtos = Database.Fetch<TemplateDto, NodeDto>(sql).ToArray();
            if (dtos.Length == 0) return Enumerable.Empty<ITemplate>();

            var axisDefos = GetAxisDefinitions(dtos).ToArray();
            return dtos.Select(x => MapFromDto(x, axisDefos));

        }

        public IEnumerable<ITemplate> GetDescendants(string alias)
        {
            var sql = GetBaseQuery(false);
            if (alias.IsNullOrWhiteSpace() == false)
            {
                //unfortunately SQLCE doesn't support scalar subqueries in the where clause, otherwise we could have done this
                // in a single query, now we have to lookup the path to acheive the same thing
                var path = Database.ExecuteScalar<string>(
                    "SELECT umbracoNode.path FROM cmsTemplate INNER JOIN umbracoNode ON cmsTemplate.nodeId = umbracoNode.id WHERE cmsTemplate.alias = @alias", new { alias = alias });

                if (path.IsNullOrWhiteSpace()) return Enumerable.Empty<ITemplate>();

                sql.Where(@"(umbracoNode." + SqlSyntax.GetQuotedColumnName("path") + @" LIKE @query)", new {query = path + ",%" });
            }

            sql.OrderBy("umbracoNode." + SqlSyntax.GetQuotedColumnName("level"));

            var dtos = Database.Fetch<TemplateDto, NodeDto>(sql).ToArray();
            if (dtos.Length == 0) return Enumerable.Empty<ITemplate>();

            var axisDefos = GetAxisDefinitions(dtos).ToArray();
            return dtos.Select(x => MapFromDto(x, axisDefos));
        }

        /// <summary>
        /// Returns a template as a template node which can be traversed (parent, children)
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        [Obsolete("Use GetDescendants instead")]
        public TemplateNode GetTemplateNode(string alias)
        {            
            //first get all template objects
            var allTemplates = GetAll().ToArray();

            var selfTemplate = allTemplates.SingleOrDefault(x => x.Alias == alias);
            if (selfTemplate == null)
            {
                return null;
            }
            
            var top = selfTemplate;
            while (top.MasterTemplateAlias.IsNullOrWhiteSpace() == false)
            {
                top = allTemplates.Single(x => x.Alias == top.MasterTemplateAlias);
            }

            var topNode = new TemplateNode(allTemplates.Single(x => x.Id == top.Id));
            var childTemplates = allTemplates.Where(x => x.MasterTemplateAlias == top.Alias);
            //This now creates the hierarchy recursively
            topNode.Children = CreateChildren(topNode, childTemplates, allTemplates);

            //now we'll return the TemplateNode requested
            return FindTemplateInTree(topNode, alias);
        }

        private static TemplateNode WalkTree(TemplateNode current, string alias)
        {
            //now walk the tree to find the node
            if (current.Template.Alias == alias)
            {
                return current;
            }
            foreach (var c in current.Children)
            {
                var found = WalkTree(c, alias);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Given a template node in a tree, this will find the template node with the given alias if it is found in the hierarchy, otherwise null
        /// </summary>
        /// <param name="anyNode"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        [Obsolete("Use GetDescendants instead")]
        public TemplateNode FindTemplateInTree(TemplateNode anyNode, string alias)
        {
            //first get the root
            var top = anyNode;
            while (top.Parent != null)
            {
                top = top.Parent;
            }
            return WalkTree(top, alias);
        }

        /// <summary>
        /// This checks what the default rendering engine is set in config but then also ensures that there isn't already 
        /// a template that exists in the opposite rendering engine's template folder, then returns the appropriate 
        /// rendering engine to use.
        /// </summary> 
        /// <returns></returns>
        /// <remarks>
        /// The reason this is required is because for example, if you have a master page file already existing under ~/masterpages/Blah.aspx
        /// and then you go to create a template in the tree called Blah and the default rendering engine is MVC, it will create a Blah.cshtml 
        /// empty template in ~/Views. This means every page that is using Blah will go to MVC and render an empty page. 
        /// This is mostly related to installing packages since packages install file templates to the file system and then create the 
        /// templates in business logic. Without this, it could cause the wrong rendering engine to be used for a package.
        /// </remarks>
        public RenderingEngine DetermineTemplateRenderingEngine(ITemplate template)
        {
            var engine = _templateConfig.DefaultRenderingEngine;
            var viewHelper = new ViewHelper(_viewsFileSystem);
            if (!viewHelper.ViewExists(template))
            {
                if (template.Content.IsNullOrWhiteSpace() == false && MasterPageHelper.IsMasterPageSyntax(template.Content))
                {
                    //there is a design but its definitely a webforms design and we haven't got a MVC view already for it
                    return RenderingEngine.WebForms;
                }
            }

            var masterPageHelper = new MasterPageHelper(_masterpagesFileSystem);

            switch (engine)
            {
                case RenderingEngine.Mvc:
                    //check if there's a view in ~/masterpages
                    if (masterPageHelper.MasterPageExists(template) && viewHelper.ViewExists(template) == false)
                    {
                        //change this to webforms since there's already a file there for this template alias
                        engine = RenderingEngine.WebForms;
                    }
                    break;
                case RenderingEngine.WebForms:
                    //check if there's a view in ~/views
                    if (viewHelper.ViewExists(template) && masterPageHelper.MasterPageExists(template) == false)
                    {
                        //change this to mvc since there's already a file there for this template alias
                        engine = RenderingEngine.Mvc;
                    }
                    break;
            }
            return engine;
        }

        /// <summary>
        /// Validates a <see cref="ITemplate"/>
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        public bool ValidateTemplate(ITemplate template)
        {
            // get path
            // TODO
            //  templates should have a real Path somehow - but anyways
            //  are we using Path for something else?!
            var path = template.VirtualPath;

            // get valid paths
            var validDirs = _templateConfig.DefaultRenderingEngine == RenderingEngine.Mvc
                ? new[] { SystemDirectories.Masterpages, SystemDirectories.MvcViews }
                : new[] { SystemDirectories.Masterpages };

            // get valid extensions
            var validExts = new List<string>();
            if (_templateConfig.DefaultRenderingEngine == RenderingEngine.Mvc)
            {
                validExts.Add("cshtml");
                validExts.Add("vbhtml");
            }
            else
            {
                validExts.Add(_templateConfig.UseAspNetMasterPages ? "master" : "aspx");
            }

            // validate path and extension
            var validFile = IOHelper.VerifyEditPath(path, validDirs);
            var validExtension = IOHelper.VerifyFileExtension(path, validExts);
            return validFile && validExtension;
        }

        private static IEnumerable<TemplateNode> CreateChildren(TemplateNode parent, IEnumerable<ITemplate> childTemplates, ITemplate[] allTemplates)
        {
            var children = new List<TemplateNode>();
            foreach (var childTemplate in childTemplates)
            {
                var template = allTemplates.Single(x => x.Id == childTemplate.Id);
                var child = new TemplateNode(template)
                    {
                        Parent = parent
                    };

                //add to our list
                children.Add(child);

                //get this node's children
                var local = childTemplate;
                var kids = allTemplates.Where(x => x.MasterTemplateAlias == local.Alias);

                //recurse
                child.Children = CreateChildren(child, kids, allTemplates);
            }
            return children;
        }

        #endregion

        /// <summary>
        /// Ensures that there are not duplicate aliases and if so, changes it to be a numbered version and also verifies the length
        /// </summary>
        /// <param name="template"></param>
        private void EnsureValidAlias(ITemplate template)
        {
            //ensure unique alias 
            template.Alias = template.Alias.ToCleanString(CleanStringType.UnderscoreAlias);

            if (template.Alias.Length > 100)
                template.Alias = template.Alias.Substring(0, 95);

            if (AliasAlreadExists(template))
            {
                template.Alias = EnsureUniqueAlias(template, 1);
            }
        }

        private bool AliasAlreadExists(ITemplate template)
        {
            var sql = GetBaseQuery(true).Where<TemplateDto>(x => x.Alias == template.Alias && x.NodeId != template.Id);
            var count = Database.ExecuteScalar<int>(sql);
            return count > 0;
        }

        private string EnsureUniqueAlias(ITemplate template, int attempts)
        {
            //TODO: This is ported from the old data layer... pretty crap way of doing this but it works for now.
            if (AliasAlreadExists(template))
                return template.Alias + attempts;
            attempts++;
            return EnsureUniqueAlias(template, attempts);
        }
    }
}