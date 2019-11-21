using NPoco;
using System;
using System.Linq;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_4_0
{
    public class AddForeignKeyIndexes : MigrationBase
    {
        public AddForeignKeyIndexes(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var newIndexes = new[]
            {
                (typeof(DictionaryDto), nameof(DictionaryDto.Parent)),
                (typeof(LanguageTextDto), nameof(LanguageTextDto.LanguageId)),
                (typeof(LanguageTextDto), nameof(LanguageTextDto.UniqueId)),
                (typeof(Member2MemberGroupDto), nameof(Member2MemberGroupDto.MemberGroup)),
                (typeof(PropertyTypeDto), nameof(PropertyTypeDto.DataTypeId)),
                (typeof(PropertyTypeDto), nameof(PropertyTypeDto.ContentTypeId)),
                (typeof(PropertyTypeDto), nameof(PropertyTypeDto.PropertyTypeGroupId)),
                (typeof(PropertyTypeGroupDto), nameof(PropertyTypeGroupDto.ContentTypeNodeId)),
                (typeof(TagRelationshipDto), nameof(TagRelationshipDto.TagId)),
                (typeof(TagRelationshipDto), nameof(TagRelationshipDto.PropertyTypeId)),
                (typeof(AccessRuleDto), nameof(AccessRuleDto.AccessId)),
                (typeof(ContentDto), nameof(ContentDto.ContentTypeId)),
                (typeof(ContentScheduleDto), nameof(ContentScheduleDto.NodeId)),
                (typeof(ContentVersionDto), nameof(ContentVersionDto.NodeId)),
                (typeof(ContentVersionDto), nameof(ContentVersionDto.UserId)),
                (typeof(ContentVersionDto), nameof(ContentVersionDto.Current)),
                (typeof(LogDto), nameof(LogDto.UserId)),
                (typeof(LogDto), nameof(LogDto.Datestamp)),
                (typeof(LogDto), nameof(LogDto.Header)),
                (typeof(NodeDto), nameof(NodeDto.UserId)),
                (typeof(RedirectUrlDto), nameof(RedirectUrlDto.ContentKey)),
                (typeof(User2NodeNotifyDto), nameof(User2NodeNotifyDto.NodeId)),
                (typeof(User2UserGroupDto), nameof(User2UserGroupDto.UserGroupId)),
                (typeof(UserGroupDto), nameof(UserGroupDto.StartContentId)),
                (typeof(UserGroupDto), nameof(UserGroupDto.StartMediaId)),
                (typeof(UserLoginDto), nameof(UserLoginDto.UserId)),
                (typeof(UserStartNodeDto), nameof(UserStartNodeDto.UserId)),
                (typeof(UserStartNodeDto), nameof(UserStartNodeDto.StartNode))
            };

            foreach (var (type, propertyName) in newIndexes)
            {
                CreateIndexIfNotExists(type, propertyName);
            }
        }

        private void CreateIndexIfNotExists(Type dto, string propertyName)
        {
            var property = dto.GetProperty(propertyName);
            var indexName = property.GetCustomAttributes(false).OfType<IndexAttribute>().Single().Name;

            if (IndexExists(indexName))
                return;

            var tableName = dto.GetField("TableName").GetValue(null) as string;
            var columnName = property.GetCustomAttributes(false).OfType<ColumnAttribute>().Single().Name;

            Create
                .Index(indexName)
                .OnTable(tableName)
                .OnColumn(columnName)
                .Ascending()
                .WithOptions().NonClustered() // All newly defined indexes are non-clustered
                .Do();
        }
    }
}
