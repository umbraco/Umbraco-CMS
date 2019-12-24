using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class UpdateDefaultMandatoryLanguage : MigrationBase
    {
        public UpdateDefaultMandatoryLanguage(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // add the new languages lock object
            AddLockObjects.EnsureLockObject(Database, Constants.Locks.Languages, "Languages");

            // get all existing languages
            var selectDtos = Sql()
                .Select<LanguageDto>()
                .From<LanguageDto>();

            var dtos = Database.Fetch<LanguageDto>(selectDtos);

            // get the id of the language which is already the default one, if any,
            // else get the lowest language id, which will become the default language
            var defaultId = int.MaxValue;
            foreach (var dto in dtos)
            {
                if (dto.IsDefault)
                {
                    defaultId = dto.Id;
                    break;
                }

                if (dto.Id < defaultId) defaultId = dto.Id;
            }

            // update, so that language with that id is now default and mandatory
            var updateDefault = Sql()
                .Update<LanguageDto>(u => u
                    .Set(x => x.IsDefault, true)
                    .Set(x => x.IsMandatory, true))
                .Where<LanguageDto>(x => x.Id == defaultId);

            Database.Execute(updateDefault);
        }
    }
}
