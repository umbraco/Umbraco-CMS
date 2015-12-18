using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeOne
{
    /// <summary>
    /// This fixes the storage of user languages from the old format like en_us to en-US
    /// </summary>
    [Migration("7.3.1", 0, GlobalSettings.UmbracoMigrationName)]
    public class UpdateUserLanguagesToIsoCode : MigrationBase
    {
        public UpdateUserLanguagesToIsoCode(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var userData = Context.Database.Fetch<UserDto>(new Sql().Select("*").From<UserDto>(SqlSyntax));
            foreach (var user in userData.Where(x => x.UserLanguage.Contains("_")))
            {
                var languageParts = user.UserLanguage.Split('_');
                if (languageParts.Length == 2)
                {
                    Update.Table("umbracoUser")
                        .Set(new {userLanguage = languageParts[0] + "-" + languageParts[1].ToUpperInvariant()})
                        .Where(new {id = user.Id});
                }
            }
        }

        public override void Down()
        {
        }
    }
}