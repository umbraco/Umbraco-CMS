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
            var dictionaryDtoIdParentIndex = $"IX_{DictionaryDto.TableName}_Id_Parent";
            CreateIndex<DictionaryDto>(dictionaryDtoIdParentIndex);
        }
    }
}
