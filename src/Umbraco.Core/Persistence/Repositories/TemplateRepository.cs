using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the Template Repository
    /// </summary>
    internal class TemplateRepository : PetaPocoRepositoryBase<int, ITemplate>, ITemplateRepository
    {
        private IFileSystem _masterpagesFileSystem;
        private IFileSystem _viewsFileSystem;

        public TemplateRepository(IDatabaseUnitOfWork work)
            : base(work)
        {
            EnsureDependencies();
        }

        public TemplateRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache)
            : base(work, cache)
        {
            EnsureDependencies();
        }

        internal TemplateRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache, IFileSystem masterpageFileSystem, IFileSystem viewFileSystem)
            : base(work, cache)
        {
            _masterpagesFileSystem = masterpageFileSystem;
            _viewsFileSystem = viewFileSystem;
        }

        private void EnsureDependencies()
        {
            _masterpagesFileSystem = new PhysicalFileSystem(SystemDirectories.Masterpages);
            _viewsFileSystem = new PhysicalFileSystem(SystemDirectories.MvcViews);
        }

        #region Overrides of RepositoryBase<int,ITemplate>

        protected override ITemplate PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.Fetch<TemplateDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            string csViewName = string.Concat(dto.Alias, ".cshtml");
            string vbViewName = string.Concat(dto.Alias, ".vbhtml");
            string masterpageName = string.Concat(dto.Alias, ".master");

            var factory = new TemplateFactory();
            var template = factory.BuildEntity(dto);

            if (dto.Master.HasValue)
            {
                var masterTemplate = Get(dto.Master.Value);
                template.MasterTemplateAlias = masterTemplate.Alias;
                template.MasterTemplateId = new Lazy<int>(() => dto.Master.Value);
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

            return template;
        }

        protected override IEnumerable<ITemplate> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                foreach (var id in ids)
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

        protected override IEnumerable<ITemplate> PerformGetByQuery(IQuery<ITemplate> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<ITemplate>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<TemplateDto, NodeDto>(sql);

            foreach (var dto in dtos.DistinctBy(x => x.NodeId))
            {
                yield return Get(dto.NodeId);
            }
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
            //TODO check for references in DocumentDto and remove value (nullable)
            var list = new List<string>
                           {
                               "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @Id",
                               "DELETE FROM umbracoUser2NodePermission WHERE nodeId = @Id",
                               "UPDATE cmsDocument SET templateId = NULL WHERE nodeId = @Id",
                               "DELETE FROM cmsDocumentType WHERE templateNodeId = @Id",
                               "DELETE FROM cmsTemplate WHERE nodeId = @Id",
                               "DELETE FROM umbracoNode WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid("6FBDE604-4178-42CE-A10B-8A2600A2F07D"); }
        }

        protected override void PersistNewItem(ITemplate entity)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(entity.Content)))
            {
                if (entity.GetTypeOfRenderingEngine() == RenderingEngine.Mvc)
                {
                    string viewName = string.Concat(entity.Alias, ".cshtml");
                    _viewsFileSystem.AddFile(viewName, stream, true);
                }
                else
                {
                    string masterpageName = string.Concat(entity.Alias, ".master");
                    _masterpagesFileSystem.AddFile(masterpageName, stream, true);
                }
            }

            //TODO Possibly ensure unique alias here (as is done in the legacy Template class)?

            //Save to db
            var template = entity as Template;
            template.AddingEntity();

            var factory = new TemplateFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(template);

            //NOTE Should the logic below have some kind of fallback for empty parent ids ?
            //Logic for setting Path, Level and SortOrder
            var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = template.ParentId });
            int level = parent.Level + 1;
            int sortOrder =
                Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE parentID = @ParentId AND nodeObjectType = @NodeObjectType",
                                                      new { ParentId = template.ParentId, NodeObjectType = NodeObjectTypeId });

            //Create the (base) node data - umbracoNode
            var nodeDto = dto.NodeDto;
            nodeDto.Path = parent.Path;
            nodeDto.Level = short.Parse(level.ToString(CultureInfo.InvariantCulture));
            nodeDto.SortOrder = sortOrder;
            var o = Database.IsNew(nodeDto) ? Convert.ToInt32(Database.Insert(nodeDto)) : Database.Update(nodeDto);

            //Update with new correct path
            nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
            Database.Update(nodeDto);

            //Insert template dto
            dto.NodeId = nodeDto.NodeId;
            Database.Insert(dto);

            //Update entity with correct values
            template.Id = nodeDto.NodeId; //Set Id on entity to ensure an Id is set
            template.Path = nodeDto.Path;
            template.SortOrder = sortOrder;
            template.Level = level;

            template.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(ITemplate entity)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(entity.Content)))
            {
                if (entity.GetTypeOfRenderingEngine() == RenderingEngine.Mvc)
                {
                    string viewName = string.Concat(entity.Alias, ".cshtml");
                    _viewsFileSystem.AddFile(viewName, stream, true);
                }
                else
                {
                    string masterpageName = string.Concat(entity.Alias, ".master");
                    _masterpagesFileSystem.AddFile(masterpageName, stream, true);
                }
            }

            //Look up parent to get and set the correct Path if ParentId has changed
            if (((ICanBeDirty)entity).IsPropertyDirty("ParentId"))
            {
                var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = ((Template)entity).ParentId });
                entity.Path = string.Concat(parent.Path, ",", entity.Id);
                ((Template)entity).Level = parent.Level + 1;
                var maxSortOrder =
                    Database.ExecuteScalar<int>(
                        "SELECT coalesce(max(sortOrder),0) FROM umbracoNode WHERE parentid = @ParentId AND nodeObjectType = @NodeObjectType",
                        new { ParentId = ((Template)entity).ParentId, NodeObjectType = NodeObjectTypeId });
                ((Template)entity).SortOrder = maxSortOrder + 1;
            }

            //Get TemplateDto from db to get the Primary key of the entity
            var templateDto = Database.SingleOrDefault<TemplateDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            //Save updated entity to db
            var template = entity as Template;
            template.UpdateDate = DateTime.Now;
            var factory = new TemplateFactory(templateDto.PrimaryKey, NodeObjectTypeId);
            var dto = factory.BuildDto(template);

            Database.Update(dto.NodeDto);
            Database.Update(dto);

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistDeletedItem(ITemplate entity)
        {
            base.PersistDeletedItem(entity);

            //Check for file under the Masterpages filesystem
            if (_masterpagesFileSystem.FileExists(entity.Name))
            {
                _masterpagesFileSystem.DeleteFile(entity.Name);
            }
            else if (_masterpagesFileSystem.FileExists(entity.Path))
            {
                _masterpagesFileSystem.DeleteFile(entity.Path);
            }

            //Check for file under the Views/Mvc filesystem
            if (_viewsFileSystem.FileExists(entity.Name))
            {
                _viewsFileSystem.DeleteFile(entity.Name);
            }
            else if (_viewsFileSystem.FileExists(entity.Path))
            {
                _viewsFileSystem.DeleteFile(entity.Path);
            }
        }

        #endregion

        private void PopulateViewTemplate(ITemplate template, string fileName)
        {
            string content = string.Empty;
            string path = string.Empty;

            using (var stream = _viewsFileSystem.OpenFile(fileName))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(bytes, 0, (int)stream.Length);
                content = Encoding.UTF8.GetString(bytes);
            }

            template.Path = _viewsFileSystem.GetRelativePath(fileName);
            template.UpdateDate = _viewsFileSystem.GetLastModified(path).UtcDateTime;
            //Currently set with db values, but will eventually be changed
            //template.CreateDate = _viewsFileSystem.GetCreated(path).UtcDateTime;
            //template.Key = new FileInfo(path).Name.EncodeAsGuid();

            template.Path = path;
            template.Content = content;
        }

        private void PopulateMasterpageTemplate(ITemplate template, string fileName)
        {
            string content = string.Empty;
            string path = string.Empty;

            using (var stream = _masterpagesFileSystem.OpenFile(fileName))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(bytes, 0, (int)stream.Length);
                content = Encoding.UTF8.GetString(bytes);
            }

            template.Path = _masterpagesFileSystem.GetRelativePath(fileName);
            template.UpdateDate = _masterpagesFileSystem.GetLastModified(path).UtcDateTime;
            //Currently set with db values, but will eventually be changed
            //template.CreateDate = _masterpagesFileSystem.GetCreated(path).UtcDateTime;
            //template.Key = new FileInfo(path).Name.EncodeAsGuid();

            template.Path = path;
            template.Content = content;
        }

        #region Implementation of ITemplateRepository

        public ITemplate Get(string alias)
        {
            var sql = GetBaseQuery(false)
                .Where<TemplateDto>(x => x.Alias == alias);

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

        #endregion
    }
}