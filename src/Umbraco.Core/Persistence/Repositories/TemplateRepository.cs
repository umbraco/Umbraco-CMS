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
            return GetAll(new[] { id }).FirstOrDefault();
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
            var childIdsSql = new Sql()
                .Select("nodeId,alias,parentID")
                .From<TemplateDto>()
                .InnerJoin<NodeDto>()
                .On<TemplateDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
                .Where<NodeDto>(t => t.ParentId > 0);
            var childIds = Database.Fetch<dynamic>(childIdsSql)
                .Select(x => new UmbracoEntity
                {
                    Id = x.nodeId,
                    ParentId = x.parentID,
                    Name = x.alias
                });

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
            var childIdsSql = new Sql()
                .Select("nodeId,alias,parentID")
                .From<TemplateDto>()
                .InnerJoin<NodeDto>()
                .On<TemplateDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
                .Where<NodeDto>(t => t.ParentId > 0);
            var childIds = Database.Fetch<dynamic>(childIdsSql)
                .Select(x => new UmbracoEntity
                {
                    Id = x.nodeId,
                    ParentId = x.parentID,
                    Name = x.alias
                });

            return dtos.Select(d => MapFromDto(d, childIds));
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,ITemplate>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
                .From<TemplateDto>()
                .InnerJoin<NodeDto>()
                .On<TemplateDto, NodeDto>(left => left.NodeId, right => right.NodeId)
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
        }

        protected override void PersistDeletedItem(ITemplate entity)
        {

            //TODO: This isn't the most ideal way to delete a template tree, because below it will actually end up 
            // recursing back to this method for each descendant and re-looking up the template list causing an extrac
            // SQL call - not ideal but there shouldn't ever be a heaping list of descendant templates.
            //The easiest way to overcome this is to expose the underlying cache upwards so that the repository has access
            // to it, then in the PersistDeletedTemplate we wouldn't recurse the underlying function, we'd just call 
            // PersistDeletedItem with a Template object and clear it's cache.

            var sql = GetBaseQuery(false).Where<NodeDto>(dto => dto.ParentId > 0 || dto.NodeId == entity.Id);

            var dtos = Database.Fetch<TemplateDto, NodeDto>(sql);

            var self = dtos.Single(x => x.NodeId == entity.Id);
            var allChildren = dtos.Except(new[] { self });
            var hierarchy = GenerateTemplateHierarchy(self, allChildren);
            //remove ourselves
            hierarchy.Remove(self);
            //change the order so it goes bottom up!
            hierarchy.Reverse();

            //delete the hierarchy
            foreach (var descendant in hierarchy)
            {
                PersistDeletedTemplate(descendant);
            }

            //now we can delete this one
            base.PersistDeletedItem(entity);

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

        private ITemplate MapFromDto(TemplateDto dto, IEnumerable<IUmbracoEntity> childDefinitions)
        {
            string csViewName = string.Concat(dto.Alias, ".cshtml");
            string vbViewName = string.Concat(dto.Alias, ".vbhtml");
            string masterpageName = string.Concat(dto.Alias, ".master");

            var factory = new TemplateFactory();
            var template = factory.BuildEntity(dto, childDefinitions);

            if (dto.NodeDto.ParentId > 0)
            {
                //TODO: Fix this n+1 query!
                var masterTemplate = Get(dto.NodeDto.ParentId);
                if (masterTemplate != null)
                {
                    template.MasterTemplateAlias = masterTemplate.Alias;
                    template.MasterTemplateId = new Lazy<int>(() => dto.NodeDto.ParentId);
                }
            }

            if (_viewsFileSystem.FileExists(csViewName))
            {
                PopulateViewTemplate(template, csViewName);
            }
            else if (_viewsFileSystem.FileExists(vbViewName))
            {
                PopulateViewTemplate(template, vbViewName);
            }
            else
            {
                if (_masterpagesFileSystem.FileExists(masterpageName))
                {
                    PopulateMasterpageTemplate(template, masterpageName);
                }
            }

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            template.ResetDirtyProperties(false);

            return template;
        }


        private void PersistDeletedTemplate(TemplateDto dto)
        {
            //we need to get the real template for this item unfortunately to remove it
            var template = Get(dto.NodeId);
            if (template != null)
            {
                //NOTE: We must cast here so that it goes to the outter method to
                // ensure the cache is updated.
                PersistDeletedItem((IEntity)template);
            }
        }

        /// <summary>
        /// Returns a list of templates in order of descendants from the parent
        /// </summary>
        /// <param name="template"></param>
        /// <param name="allChildTemplates"></param>
        /// <returns></returns>
        private static List<TemplateDto> GenerateTemplateHierarchy(TemplateDto template, IEnumerable<TemplateDto> allChildTemplates)
        {
            var hierarchy = new List<TemplateDto> { template };
            foreach (var t in allChildTemplates.Where(x => x.NodeDto.ParentId == template.NodeId))
            {
                hierarchy.AddRange(GenerateTemplateHierarchy(t, allChildTemplates));
            }
            return hierarchy;
        }

        private void PopulateViewTemplate(ITemplate template, string fileName)
        {
            string content;

            using (var stream = _viewsFileSystem.OpenFile(fileName))
            using (var reader = new StreamReader(stream, Encoding.UTF8, true))
            {
                content = reader.ReadToEnd();
            }
            template.UpdateDate = _viewsFileSystem.GetLastModified(fileName).UtcDateTime;
            template.Content = content;
            template.VirtualPath = _viewsFileSystem.GetUrl(fileName);
        }

        private void PopulateMasterpageTemplate(ITemplate template, string fileName)
        {
            string content;
            
            using (var stream = _masterpagesFileSystem.OpenFile(fileName))
            using (var reader = new StreamReader(stream, Encoding.UTF8, true))
            {
                content = reader.ReadToEnd();
            }

            template.UpdateDate = _masterpagesFileSystem.GetLastModified(fileName).UtcDateTime;
            template.Content = content;
            template.VirtualPath = _masterpagesFileSystem.GetUrl(fileName);
        }

        #region Implementation of ITemplateRepository

        public ITemplate Get(string alias)
        {
            var sql = GetBaseQuery(false).Where<TemplateDto>(x => x.Alias == alias);

            var dto = Database.Fetch<TemplateDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            return Get(dto.NodeId);
        }

        public IEnumerable<ITemplate> GetAll(params string[] aliases)
        {
            if (aliases.Any())
            {
                foreach (var id in aliases)
                {
                    yield return Get(id);
                }
            }
            else
            {
                var nodeDtos = Database.Fetch<NodeDto>("WHERE nodeObjectType = @NodeObjectType", new { NodeObjectType = NodeObjectTypeId });
                foreach (var nodeDto in nodeDtos)
                {
                    yield return Get(nodeDto.NodeId);
                }
            }

        }

        public IEnumerable<ITemplate> GetChildren(int masterTemplateId)
        {
            //TODO: Fix this N+1!

            List<TemplateDto> found;
            if (masterTemplateId == -1)
            {
                var sql = GetBaseQuery(false).Where<NodeDto>(x => x.ParentId <= 0);
                found = Database.Fetch<TemplateDto, NodeDto>(sql);
            }
            else
            {
                var sql = GetBaseQuery(false).Where<NodeDto>(x => x.ParentId == masterTemplateId);
                found = Database.Fetch<TemplateDto, NodeDto>(sql);
            }

            foreach (var templateDto in found)
            {
                yield return Get(templateDto.NodeId);
            }
        }

        /// <summary>
        /// Returns a template as a template node which can be traversed (parent, children)
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public TemplateNode GetTemplateNode(string alias)
        {
            //in order to do this we need to get all of the templates and then organize, 
            // TODO: unfortunately our db structure does not use the path correctly for templates so we cannot just look
            // up a template tree easily.
            //TODO: We do use the 'path' now, so this might be able to be fixed up 

            //first get all template objects
            var allTemplates = GetAll().ToArray();

            var selfTemplate = allTemplates.SingleOrDefault(x => x.Alias == alias);
            if (selfTemplate == null)
            {
                return null;
            }

            //then we need to get all template Dto's because those contain the master property
            var sql = GetBaseQuery(false);
            var allDtos = Database.Fetch<TemplateDto, NodeDto>(sql).ToArray();
            var selfDto = allDtos.Single(x => x.NodeId == selfTemplate.Id);

            //need to get the top-most node of the current tree
            var top = selfDto;
            while (top.NodeDto.ParentId > 0)
            {
                top = allDtos.Single(x => x.NodeId == top.NodeDto.ParentId);
            }

            var topNode = new TemplateNode(allTemplates.Single(x => x.Id == top.NodeId));
            var childIds = allDtos.Where(x => x.NodeDto.ParentId == top.NodeId).Select(x => x.NodeId);
            //This now creates the hierarchy recursively
            topNode.Children = CreateChildren(topNode, childIds, allTemplates, allDtos);

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

            if (template.Content.IsNullOrWhiteSpace() == false && MasterPageHelper.IsMasterPageSyntax(template.Content))
            {
                //there is a design but its definitely a webforms design
                return RenderingEngine.WebForms;
            }

            var viewHelper = new ViewHelper(_viewsFileSystem);
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
            var exts = new List<string>();
            if (_templateConfig.DefaultRenderingEngine == RenderingEngine.Mvc)
            {
                exts.Add("cshtml");
                exts.Add("vbhtml");
            }
            else
            {
                exts.Add(_templateConfig.UseAspNetMasterPages ? "master" : "aspx");
            }

            var dirs = SystemDirectories.Masterpages;
            if (_templateConfig.DefaultRenderingEngine == RenderingEngine.Mvc)
                dirs += "," + SystemDirectories.MvcViews;

            //Validate file
            var validFile = IOHelper.VerifyEditPath(template.VirtualPath, dirs.Split(','));

            //Validate extension
            var validExtension = IOHelper.VerifyFileExtension(template.VirtualPath, exts);

            return validFile && validExtension;
        }

        private static IEnumerable<TemplateNode> CreateChildren(TemplateNode parent, IEnumerable<int> childIds, ITemplate[] allTemplates, TemplateDto[] allDtos)
        {
            var children = new List<TemplateNode>();
            foreach (var i in childIds)
            {
                var template = allTemplates.Single(x => x.Id == i);
                var child = new TemplateNode(template)
                    {
                        Parent = parent
                    };

                //add to our list
                children.Add(child);

                //get this node's children
                var kids = allDtos.Where(x => x.NodeDto.ParentId == i).Select(x => x.NodeId).ToArray();

                //recurse
                child.Children = CreateChildren(child, kids, allTemplates, allDtos);
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