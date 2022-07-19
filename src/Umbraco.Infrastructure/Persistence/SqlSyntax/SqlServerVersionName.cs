namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

/// <summary>
///     Represents the version name of SQL server (i.e. the year 2008, 2005, etc...)
/// </summary>
/// <remarks>
///     see: https://support.microsoft.com/en-us/kb/321185
/// </remarks>
internal enum SqlServerVersionName
{
    Invalid = -1,
    V7 = 0,
    V2000 = 1,
    V2005 = 2,
    V2008 = 3,
    V2012 = 4,
    V2014 = 5,
    V2016 = 6,
    Other = 100,
}
