using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class UpdateDefaultMandatoryLanguage : MigrationBase
{
    public UpdateDefaultMandatoryLanguage(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // add the new languages lock object
        AddLockObjects.EnsureLockObject(Database, Constants.Locks.Languages, "Languages");

        // get all existing languages
        Sql<ISqlContext> selectDtos = Sql()
            .Select<LanguageDto>()
            .From<LanguageDto>();

        List<LanguageDto>? dtos = Database.Fetch<LanguageDto>(selectDtos);

        // get the id of the language which is already the default one, if any,
        // else get the lowest language id, which will become the default language
        var defaultId = int.MaxValue;
        foreach (LanguageDto? dto in dtos)
        {
            if (dto.IsDefault)
            {
                defaultId = dto.Id;
                break;
            }

            if (dto.Id < defaultId)
            {
                defaultId = dto.Id;
            }
        }

        // update, so that language with that id is now default and mandatory
        Sql<ISqlContext> updateDefault = Sql()
            .Update<LanguageDto>(u => u
                .Set(x => x.IsDefault, true)
                .Set(x => x.IsMandatory, true))
            .Where<LanguageDto>(x => x.Id == defaultId);

        Database.Execute(updateDefault);
    }
}
