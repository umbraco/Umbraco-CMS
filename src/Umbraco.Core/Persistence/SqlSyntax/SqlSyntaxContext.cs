using System;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Singleton to handle the configuration of a SqlSyntaxProvider
    /// </summary>
    [Obsolete("This should not be used, the ISqlSyntaxProvider is part of the DatabaseContext or should be injected into your services as a constructor parameter")]
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