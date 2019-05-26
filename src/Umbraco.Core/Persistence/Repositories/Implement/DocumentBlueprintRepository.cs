using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{

    /// <summary>
    /// Override the base content repository so we can change the node object type
    /// </summary>
    /// <remarks>
    /// It would be nicer if we could separate most of this down into a smaller version of the ContentRepository class, however to do that
    /// requires quite a lot of work since we'd need to re-organize the inheritance quite a lot or create a helper class to perform a lot of the underlying logic.
    ///
    /// TODO: Create a helper method to contain most of the underlying logic for the ContentRepository
    /// </remarks>
    internal class DocumentBlueprintRepository : DocumentRepository, IDocumentBlueprintRepository
    {
        public DocumentBlueprintRepository(IScopeAccessor scopeAccessor, AppCaches appCaches, ILogger logger, IContentTypeRepository contentTypeRepository, ITemplateRepository templateRepository, ITagRepository tagRepository, ILanguageRepository languageRepository)
            : base(scopeAccessor, appCaches, logger, contentTypeRepository, templateRepository, tagRepository, languageRepository)
        {
        }

        protected override bool EnsureUniqueNaming => false; // duplicates are allowed

        protected override Guid NodeObjectTypeId => Constants.ObjectTypes.DocumentBlueprint;

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = base.GetDeleteClauses().ToList();
            list.Insert(0, "DELETE FROM " + Constants.DatabaseSchema.Tables.UserGroup2ContentTemplate + " WHERE nodeId = @id");
            return list;
        }

        public IEnumerable<IUserGroup> GetGroupsAssignedToBlueprint(int id)
        {
            var sql = Sql()
                .Select<UserGroupDto>()
                .From<UserGroupDto>()
                .InnerJoin<UserGroup2ContentTemplateDto>()
                .On<UserGroupDto, UserGroup2ContentTemplateDto>(left => left.Id, right => right.UserGroupId)
                .Where<UserGroup2ContentTemplateDto>(x => x.NodeId == id);
            return Database.Fetch<UserGroupDto>(sql).Select(UserGroupFactory.BuildEntity).OrderBy(x => x.Name);
        }

        public void AssignGroupsToBlueprint(int id, int[] userGroupIds)
        {
            var sql = Sql().Delete<UserGroup2ContentTemplateDto>().Where<UserGroup2ContentTemplateDto>(x => x.NodeId == id);
            Database.Execute(sql);
            if (userGroupIds == null)
            {
                return;
            }

            foreach (var userGroupId in userGroupIds)
            {
                var dto = new UserGroup2ContentTemplateDto
                    {
                        NodeId = id,
                        UserGroupId = userGroupId
                    };
                Database.Insert(dto);
            }
        }
    }
}
