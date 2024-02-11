using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V13_3_0
{
    public class AddSqlIndexes : MigrationBase
    {
        public AddSqlIndexes(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            var dictionaryDtoIdParentIdx = $"IX_{DictionaryDto.TableName}_Id_Parent";
            CreateIndex<DictionaryDto>(dictionaryDtoIdParentIdx);

            var contentTypeNodeIdIdx = $"IX_{ContentTypeDto.TableName}_nodeId";
            CreateIndex<ContentTypeDto>(contentTypeNodeIdIdx);

            var twoFactorUserOrMemberKeyIdx = $"IX_{TwoFactorLoginDto.TableName}_userOrMemberKey";
            DeleteIndex<TwoFactorLoginDto>(twoFactorUserOrMemberKeyIdx);
            CreateIndex<TwoFactorLoginDto>(twoFactorUserOrMemberKeyIdx);

            var contentTypeNodeIdx = $"IX_{PropertyTypeGroupDto.TableName}_contenttypeNodeId";
            CreateIndex<PropertyTypeGroupDto>(contentTypeNodeIdx);

            var contentVersionIdx = $"IX_{ContentVersionDto.TableName}_current_versionDate_desc_nodeId";
            CreateIndex<PropertyTypeGroupDto>(contentVersionIdx);

            var umbracoNodeLevelIdx = $"IX_{NodeDto.TableName}_Level";
            DeleteIndex<NodeDto>(umbracoNodeLevelIdx);
            CreateIndex<NodeDto>(umbracoNodeLevelIdx);

            var macroPropertyMacroIdx = $"IX_{MacroPropertyDto.TableName}_macro";
            CreateIndex<MacroPropertyDto>(macroPropertyMacroIdx);

            var umbracoContentScheduleActionDateIdx = $"IX_{ContentScheduleDto.TableName}_action_date";
            CreateIndex<ContentScheduleDto>(umbracoContentScheduleActionDateIdx);

            var umbracoRelationIdx = $"IX_{RelationDto.TableName}_reltype";
            CreateIndex<RelationDto>(umbracoRelationIdx);

            var umbracoNodeObjectTypeLevelSortOrderIdx = $"IX_{NodeDto.TableName}_ObjectType_nodeObjectType_level_sortOrder";
            CreateIndex<NodeDto>(umbracoNodeObjectTypeLevelSortOrderIdx);

            var umbracoDomainSortOrderId = $"IX_{DomainDto.TableName}_sortOrder_id";
            CreateIndex<DomainDto>(umbracoDomainSortOrderId);

            var umbracoUserNameIdx = $"IX_{UserDto.TableName}_userName";
            CreateIndex<UserDto>(umbracoUserNameIdx);

            var umbracoContentType2ContentTypeIdx = $"IX_{ContentType2ContentTypeDto.TableName}_childContentTypeId";
            CreateIndex<ContentType2ContentTypeDto>(umbracoContentType2ContentTypeIdx);
        }
    }
}
