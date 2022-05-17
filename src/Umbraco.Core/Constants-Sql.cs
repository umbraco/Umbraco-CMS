namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class Sql
    {
        /// <summary>
        ///     The maximum amount of parameters that can be used in a query.
        /// </summary>
        /// <remarks>
        ///     The actual limit is 2100
        ///     (https://docs.microsoft.com/en-us/sql/sql-server/maximum-capacity-specifications-for-sql-server),
        ///     but we want to ensure there's room for additional parameters if this value is used to create groups/batches.
        /// </remarks>
        public const int MaxParameterCount = 2000;
    }
}
