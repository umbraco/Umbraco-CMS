namespace Umbraco.Cms.Persistence.EFCore.Migrations;

/// <summary>
/// Enumerates the Entity Framework Core migration steps for Umbraco CMS.
/// </summary>
public enum EFCoreMigration
{
    InitialCreate = 0,
    AddOpenIddict = 1,
    UpdateOpenIddictToV5 = 2,
    UpdateOpenIddictToV7 = 3,
}
