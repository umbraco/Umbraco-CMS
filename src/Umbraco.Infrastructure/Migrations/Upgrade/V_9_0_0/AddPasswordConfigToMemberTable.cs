﻿using System.Linq;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0
{
    public class AddPasswordConfigToMemberTable : MigrationBase
    {
        public AddPasswordConfigToMemberTable(IMigrationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Adds new columns to members table
        /// </summary>
        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

            AddColumnIfNotExists<MemberDto>(columns, "passwordConfig");
        }
    }
}
