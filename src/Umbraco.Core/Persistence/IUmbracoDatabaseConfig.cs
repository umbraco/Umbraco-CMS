using NPoco;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    public interface IUmbracoDatabaseConfig : IDatabaseConfig
    {
        /// <summary>
        /// The Umbraco SqlSyntax used to handle different syntaxes in the different database providers.
        /// </summary>
        ISqlSyntaxProvider SqlSyntax { get; }
    }
}
