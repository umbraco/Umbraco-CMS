using System;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Singleton to handle the configuration of a SqlSyntaxProvider
    /// </summary>
    public static class SqlSyntaxContext
    {
        private static ISqlSyntaxProvider _sqlSyntaxProvider;

        public static ISqlSyntaxProvider SqlSyntaxProvider
        {
            get
            {
                if(_sqlSyntaxProvider == null)
                {
                    throw new ArgumentNullException("SqlSyntaxProvider",
                                                    "You must set the singleton 'Umbraco.Core.Persistence.SqlSyntax.SqlSyntaxContext' to use an sql syntax provider");
                }
                return _sqlSyntaxProvider;
            }
            set { _sqlSyntaxProvider = value; }
        }
    }
}