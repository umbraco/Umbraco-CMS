namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Static class that provides simple access to the MySql SqlSyntax Provider
    /// </summary>
    internal static class MySqlSyntax
    {
        public static ISqlSyntaxProvider Provider { get { return new MySqlSyntaxProvider(); } }
    }
}