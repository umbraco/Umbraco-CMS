namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Static class that provides simple access to the Sql CE SqlSyntax Provider
    /// </summary>
    internal static class SqlCeSyntax
    {
        public static ISqlSyntaxProvider Provider { get { return new SqlCeSyntaxProvider(); } }
    }
}