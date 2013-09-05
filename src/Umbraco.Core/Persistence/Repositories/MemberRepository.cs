using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class MemberRepository : VersionableRepositoryBase<int, IMembershipUser>, IMemberRepository
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
         * GetByPropertyValue - get members with a certain property value (supply both property alias and value?)
         * GetByMemberTypeAlias - get all members of a certain type
         * GetByMemberGroup - get all members in a specific group
         * GetAllMembers
         */

        #region Overrides of RepositoryBase<int, IMembershipUser>

        protected override IMembershipUser PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto =
                Database.Fetch<MemberReadOnlyDto, PropertyDataReadOnlyDto, MemberReadOnlyDto>(
                    new PropertyDataRelator().Map, sql);

            if (dto == null || dto.Any() == false)
                return null;

            return new Member();
        }

        protected override IEnumerable<IMembershipUser> PerformGetAll(params int[] ids)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IMembershipUser> PerformGetByQuery(IQuery<IMembershipUser> query)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,IMembershipUser>

        protected override Sql GetBaseQuery(bool isCount)
        {
            //TODO Count
            var sql = new Sql();
            sql.Select("umbracoNode.*", "cmsContentType.alias AS ContentTypeAlias", "cmsContentVersion.VersionId",
                "cmsContentVersion.VersionDate", "cmsContentVersion.LanguageLocale", "cmsMember.Email",
                "cmsMember.LoginName", "cmsMember.Password", "cmsPropertyData.id", "cmsPropertyData.dataDate",
                "cmsPropertyData.dataInt", "cmsPropertyData.dataNtext", "cmsPropertyData.dataNvarchar",
                "cmsPropertyData.propertytypeid", "cmsPropertyType.Alias", "cmsPropertyType.Description",
                "cmsPropertyType.Name", "cmsPropertyType.mandatory", "cmsPropertyType.validationRegExp",
                "cmsPropertyType.helpText", "cmsPropertyType.propertyTypeGroupId", "cmsPropertyType.dataTypeId",
                "cmsDataType.propertyEditorAlias")
                .From<NodeDto>()
                .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<PropertyDataDto>().On<PropertyDataDto, ContentVersionDto>(left => left.VersionId, right => right.VersionId)
                .LeftJoin<PropertyTypeDto>().On<PropertyTypeDto, PropertyDataDto>(left => left.Id, right => right.PropertyTypeId)
                .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            throw new NotImplementedException();
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.Member); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IMembershipUser entity)
        {
            throw new NotImplementedException();
        }

        protected override void PersistUpdatedItem(IMembershipUser entity)
        {
            throw new NotImplementedException();
        }
        
        #endregion

        #region Overrides of VersionableRepositoryBase<IMembershipUser>

        public override IMembershipUser GetByVersion(Guid versionId)
        {
            throw new NotImplementedException();
        }

        protected override void PerformDeleteVersion(int id, Guid versionId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}