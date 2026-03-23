namespace Umbraco.Cms.Persistence.EFCore.Migrations;

public enum EFCoreMigration
{
    InitialCreate = 0,
    AddOpenIddict = 1,
    UpdateOpenIddictToV5 = 2,
    UpdateOpenIddictToV7 = 3,
    AddWebhookDto = 4,
    AddLastSyncedDto = 5,
    AddKeyValueDto = 6,
    SqliteCollation = 7,
    AddLanguageDto = 8,
}
