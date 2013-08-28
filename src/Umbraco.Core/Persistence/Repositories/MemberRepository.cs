using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class MemberRepository : VersionableRepositoryBase<int, IMember>, IMemberRepository
    {
        public MemberRepository(IDatabaseUnitOfWork work) : base(work)
        {
        }

        public MemberRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
        }

        /* The following methods might be relevant for this specific repository (in an optimized form)
         * GetById - get a member by its integer Id
         * GetByKey - get a member by its unique guid Id (which should correspond to a membership provider user's id)
         * GetByUsername - get a member by its username
         * 
         * GetByPropertyValue - get members with a certain property value (supply both property alias and value?)
         * GetByMemberTypeAlias - get all members of a certain type
         * GetByMemberGroup - get all members in a specific group
         */

        #region Overrides of RepositoryBase<int, IMembershipUser>

        protected override IMember PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto =
                Database.Fetch<MemberReadOnlyDto, PropertyDataReadOnlyDto, MemberReadOnlyDto>(
                    new PropertyDataRelator().Map, sql);

            return BuildFromDto(dto);
        }

        protected override IEnumerable<IMember> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);
            if (ids.Any())
            {
                var statement = string.Join(" OR ", ids.Select(x => string.Format("umbracoNode.id='{0}'", x)));
                sql.Where(statement);
            }
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dtos =
                Database.Fetch<MemberReadOnlyDto, PropertyDataReadOnlyDto, MemberReadOnlyDto>(
                    new PropertyDataRelator().Map, sql);

            return BuildFromDtos(dtos);
        }

        protected override IEnumerable<IMember> PerformGetByQuery(IQuery<IMember> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IMember>(sqlClause, query);
            var sql = translator.Translate()
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate)
                .OrderBy<NodeDto>(x => x.SortOrder);

            var dtos =
                Database.Fetch<MemberReadOnlyDto, PropertyDataReadOnlyDto, MemberReadOnlyDto>(
                    new PropertyDataRelator().Map, sql);

            return BuildFromDtos(dtos);
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,IMembershipUser>

        protected override Sql GetBaseQuery(bool isCount)
        {
            if (isCount)
            {
                var sqlCount = new Sql()
                    .Select("COUNT(*)")
                    .From<NodeDto>()
                    .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                    .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
                return sqlCount;
            }

            var sql = new Sql();
            sql.Select("umbracoNode.*", "cmsContent.contentType", "cmsContentType.alias AS ContentTypeAlias", "cmsContentVersion.VersionId",
                "cmsContentVersion.VersionDate", "cmsContentVersion.LanguageLocale", "cmsMember.Email",
                "cmsMember.LoginName", "cmsMember.Password", "cmsPropertyData.id AS PropertyDataId", "cmsPropertyData.propertytypeid", 
                "cmsPropertyData.dataDate", "cmsPropertyData.dataInt", "cmsPropertyData.dataNtext", "cmsPropertyData.dataNvarchar",
                "cmsPropertyType.id", "cmsPropertyType.Alias", "cmsPropertyType.Description",
                "cmsPropertyType.Name", "cmsPropertyType.mandatory", "cmsPropertyType.validationRegExp",
                "cmsPropertyType.helpText", "cmsPropertyType.sortOrder AS PropertyTypeSortOrder", "cmsPropertyType.propertyTypeGroupId", 
                "cmsPropertyType.dataTypeId", "cmsDataType.controlId", "cmsDataType.dbType")
                .From<NodeDto>()
                .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<PropertyTypeDto>().On<PropertyTypeDto, ContentDto>(left => left.ContentTypeId, right => right.ContentTypeId)
                .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
                .LeftJoin<PropertyDataDto>().On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .Append("AND cmsPropertyData.versionId = cmsContentVersion.VersionId")
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
                               "DELETE FROM umbracoRelation WHERE parentId = @Id",
                               "DELETE FROM umbracoRelation WHERE childId = @Id",
                               "DELETE FROM cmsTagRelationship WHERE nodeId = @Id",
                               "DELETE FROM cmsPropertyData WHERE contentNodeId = @Id",
                               "DELETE FROM cmsMember WHERE nodeId = @Id",
                               "DELETE FROM cmsContentVersion WHERE ContentId = @Id",
                               "DELETE FROM cmsContentXml WHERE nodeID = @Id",
                               "DELETE FROM cmsContent WHERE NodeId = @Id",
                               "DELETE FROM umbracoNode WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.Member); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IMember entity)
        {
            throw new NotImplementedException();
        }

        protected override void PersistUpdatedItem(IMember entity)
        {
            throw new NotImplementedException();
        }
        
        #endregion

        #region Overrides of VersionableRepositoryBase<IMembershipUser>

        public override IMember GetByVersion(Guid versionId)
        {
            throw new NotImplementedException();
        }

        protected override void PerformDeleteVersion(int id, Guid versionId)
        {
            throw new NotImplementedException();
        }

        #endregion

        //NOTE Might be sufficient to use the GetByQuery method for this, as the mapping should cover it
        public IMember GetByKey(Guid key)
        {
            var sql = GetBaseQuery(false);
            sql.Where<NodeDto>(x => x.UniqueId == key);
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto =
                Database.Fetch<MemberReadOnlyDto, PropertyDataReadOnlyDto, MemberReadOnlyDto>(
                    new PropertyDataRelator().Map, sql);

            return BuildFromDto(dto);
        }

        //NOTE Might be sufficient to use the GetByQuery method for this, as the mapping should cover it
        public IMember GetByUsername(string username)
        {
            var sql = GetBaseQuery(false);
            sql.Where<MemberDto>(x => x.LoginName == username);
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto =
                Database.Fetch<MemberReadOnlyDto, PropertyDataReadOnlyDto, MemberReadOnlyDto>(
                    new PropertyDataRelator().Map, sql);

            return BuildFromDto(dto);
        }

        //NOTE Might be sufficient to use the GetByQuery method for this, as the mapping should cover it
        public IEnumerable<IMember> GetByMemberTypeAlias(string memberTypeAlias)
        {
            var sql = GetBaseQuery(false);
            sql.Where<ContentTypeDto>(x => x.Alias == memberTypeAlias);
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dtos =
                Database.Fetch<MemberReadOnlyDto, PropertyDataReadOnlyDto, MemberReadOnlyDto>(
                    new PropertyDataRelator().Map, sql);

            return BuildFromDtos(dtos);
        }

        /*public IEnumerable<IMember> GetByPropertyValue(string value)
        {}

        public IEnumerable<IMember> GetByPropertyValue(bool value)
        { }

        public IEnumerable<IMember> GetByPropertyValue(int value)
        { }

        public IEnumerable<IMember> GetByPropertyValue(DateTime value)
        { }

        public IEnumerable<IMember> GetByPropertyValue(string propertyTypeAlias, string value)
        { }

        public IEnumerable<IMember> GetByPropertyValue(string propertyTypeAlias, bool value)
        { }

        public IEnumerable<IMember> GetByPropertyValue(string propertyTypeAlias, int value)
        { }

        public IEnumerable<IMember> GetByPropertyValue(string propertyTypeAlias, DateTime value)
        { }*/

        private IMember BuildFromDto(List<MemberReadOnlyDto> dtos)
        {
            if (dtos == null || dtos.Any() == false)
                return null;

            var factory = new MemberReadOnlyFactory();
            var member = factory.BuildEntity(dtos.First());

            return member;
        }

        private IEnumerable<IMember> BuildFromDtos(List<MemberReadOnlyDto> dtos)
        {
            if (dtos == null || dtos.Any() == false)
                return Enumerable.Empty<IMember>();

            var factory = new MemberReadOnlyFactory();
            return dtos.Select(factory.BuildEntity);
        }
    }
}