using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0
{
    public class ExternalLoginTableIndexes : MigrationBase
    {
        public ExternalLoginTableIndexes(IMigrationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Adds new indexes to the External Login table
        /// </summary>
        public override void Migrate()
        {
            // Before adding these indexes we need to remove duplicate data.
            // Get all logins by latest
            var logins = Database.Fetch<ExternalLoginDto>()
                .OrderByDescending(x => x.CreateDate)
                .ToList();

            var toDelete = new List<int>();
            // used to track duplicates so they can be removed
            var keys = new HashSet<(string, string)>();
            foreach(ExternalLoginDto login in logins)
            {
                if (!keys.Add((login.ProviderKey, login.LoginProvider)))
                {
                    // if it already exists we need to remove this one
                    toDelete.Add(login.Id);
                }
            }
            if (toDelete.Count > 0)
            {
                Database.DeleteMany<ExternalLoginDto>().Where(x => toDelete.Contains(x.Id)).Execute();
            }   

            var indexName1 = "IX_" + ExternalLoginDto.TableName + "_LoginProvider";

            if (!IndexExists(indexName1))
            {
                Create
                     .Index(indexName1)
                     .OnTable(ExternalLoginDto.TableName)
                     .OnColumn("loginProvider")
                     .Ascending()
                     .WithOptions()
                     .Unique()
                     .WithOptions()
                     .NonClustered()
                     .Do();
            }

            var indexName2 = "IX_" + ExternalLoginDto.TableName + "_ProviderKey";

            if (!IndexExists(indexName2))
            {
                Create
                     .Index(indexName2)
                     .OnTable(ExternalLoginDto.TableName)
                     .OnColumn("loginProvider").Ascending()
                     .OnColumn("providerKey").Ascending()
                     .WithOptions()
                     .NonClustered()
                     .Do();
            }
        }
    }
}
