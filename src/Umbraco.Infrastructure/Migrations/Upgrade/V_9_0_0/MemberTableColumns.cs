﻿using System.Linq;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0
{
    public class MemberTableColumns : MigrationBase
    {
        public MemberTableColumns(IMigrationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Adds new External Login token table
        /// </summary>
        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

            AddColumnIfNotExists<MemberDto>(columns, "securityStampToken");
            AddColumnIfNotExists<MemberDto>(columns, "emailConfirmedDate");
        }
    }
}
