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
            // fixme - ends up creating an index on a not-yet-existing column!
            // LanguageColumn migration has run but FallbackLanguage has not
            // how can we manage this?
            // a) creating and dropping all keys is ugly
            //    can we stop dropping them all?
            // b) should get captures of Dto objects and use them in migrations

            //Create.KeysAndIndexes<LanguageDto>().Do();
            //Create.KeysAndIndexes<NodeDto>().Do();
            //Create.KeysAndIndexes<ContentTypeDto>().Do();
            //Create.KeysAndIndexes<ContentDto>().Do();
            //Create.KeysAndIndexes<ContentVersionDto>().Do();

            Create.Table<ContentVersionCultureVariationDto>(true).Do();
            Create.Table<DocumentCultureVariationDto>(true).Do();

            //Delete.KeysAndIndexes(ContentVersionCultureVariationDto.TableName).Do();
            //Delete.KeysAndIndexes(DocumentCultureVariationDto.TableName).Do();

            //Delete.KeysAndIndexes(LanguageDto.TableName).Do();
            //Delete.KeysAndIndexes(ContentVersionDto.TableName).Do();
            //Delete.KeysAndIndexes(ContentDto.TableName).Do();
            //Delete.KeysAndIndexes(ContentTypeDto.TableName).Do();
            //Delete.KeysAndIndexes(NodeDto.TableName).Do();
        }
    }
}
