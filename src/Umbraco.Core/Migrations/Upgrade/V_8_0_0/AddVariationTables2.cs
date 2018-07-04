using System;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class AddVariationTables2 : MigrationBase
    {
        public AddVariationTables2(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // fixme
            Create.KeysAndIndexes<LanguageDto>().Do();
            Create.KeysAndIndexes<NodeDto>().Do();
            Create.KeysAndIndexes<ContentTypeDto>().Do();
            Create.KeysAndIndexes<ContentDto>().Do();
            Create.KeysAndIndexes<ContentVersionDto>().Do();

            Create.Table<ContentVersionCultureVariationDto>().Do();
            Create.Table<DocumentCultureVariationDto>().Do();

            Delete.KeysAndIndexes(ContentVersionCultureVariationDto.TableName).Do();
            Delete.KeysAndIndexes(DocumentCultureVariationDto.TableName).Do();

            Delete.KeysAndIndexes(LanguageDto.TableName).Do();
            Delete.KeysAndIndexes(ContentVersionDto.TableName).Do();
            Delete.KeysAndIndexes(ContentDto.TableName).Do();
            Delete.KeysAndIndexes(ContentTypeDto.TableName).Do();
            Delete.KeysAndIndexes(NodeDto.TableName).Do();

        }
    }
}
