namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Static class that provides simple access to the Sql Server SqlSyntax Provider
    /// </summary>
    internal static class SqlServerSyntax
    {
        public static ISqlSyntaxProvider Provider { get { return new SqlServerSyntaxProvider(); } }
    }
}