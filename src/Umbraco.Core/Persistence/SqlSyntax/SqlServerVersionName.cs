namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Represents the version name of SQL server (i.e. the year 2008, 2005, etc...)
    /// </summary>
    internal enum SqlServerVersionName
    {
        Invalid = -1,
        V7 = 0,
        V2000 = 1,
        V2005 = 2,
        V2008 = 3,
        V2012 = 4,
        Other = 5
    }
}