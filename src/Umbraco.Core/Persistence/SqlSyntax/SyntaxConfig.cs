using System;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Singleton to handle the configuration of an SqlSyntaxProvider
    /// </summary>
    internal static class SyntaxConfig
    {
        private static ISqlSyntaxProvider _sqlSyntaxProvider;

        public static ISqlSyntaxProvider SqlSyntaxProvider
        {
            get
            {
                if(_sqlSyntaxProvider == null)
                {
                    throw new ArgumentNullException("SqlSyntaxProvider",
                                                    "You must set the singleton 'Umbraco.Core.Persistence.SqlSyntax.SyntaxConfig' to use an sql syntax provider");
                }
                return _sqlSyntaxProvider;
            }
            set { _sqlSyntaxProvider = value; }
        }
    }
}