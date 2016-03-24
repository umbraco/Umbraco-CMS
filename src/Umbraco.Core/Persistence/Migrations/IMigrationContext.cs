using System.Collections.Generic;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations
{
    public interface IMigrationContext
    {
        ICollection<IMigrationExpression> Expressions { get; set; }
        DatabaseProviders CurrentDatabaseProvider { get; }
        Database Database { get; }
        ISqlSyntaxProvider SqlSyntax { get; }
    }
}