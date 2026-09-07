using NUnit.Framework;
using Umbraco.Cms.Persistence.SqlServer.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.SqlServer;

[TestFixture]
public class SqlServerDatabaseCreatorTests
{
    // The expected values below are the names SQL Server itself derives when CREATE DATABASE
    // specifies only a data file, verified against SQL Server 2019 and 2025 LocalDB. The log file
    // is named after the data file rather than the database, and only the last extension is
    // stripped, whatever that extension is.
    [Test]
    [TestCase("Umbraco.mdf", ExpectedResult = "Umbraco_log")]
    [TestCase("Umbraco.MDF", ExpectedResult = "Umbraco_log")]
    [TestCase("Umbraco.data", ExpectedResult = "Umbraco_log")]
    [TestCase("Umbraco", ExpectedResult = "Umbraco_log")]
    [TestCase("My.Site.mdf", ExpectedResult = "My.Site_log")]
    [TestCase(".mdf", ExpectedResult = "_log")]
    public string Can_Derive_The_Log_Name_Sql_Server_Would_Derive(string dataFileName)
        => SqlServerDatabaseCreator.GetLogName(dataFileName);

    [Test]
    public void Can_Derive_The_Log_File_Next_To_The_Data_File()
    {
        var dataFileName = Path.Combine("umbraco", "Data", "Umbraco.mdf");

        Assert.AreEqual(
            Path.Combine("umbraco", "Data", "Umbraco_log.ldf"),
            SqlServerDatabaseCreator.GetLogFileName(dataFileName));
    }

    [Test]
    public void Can_Derive_The_Log_File_In_A_Directory_Whose_Name_Starts_With_A_Dot()
    {
        // SQL Server's own derivation drops the separator in front of ".tools" here, which is why
        // the log file is specified explicitly: it would otherwise resolve to a "repo.tools"
        // directory that does not exist, and CREATE DATABASE would fail with operating system
        // error 3.
        var dataFileName = Path.Combine("repo", ".tools", "wt", "umbraco", "Data", "Umbraco.mdf");

        Assert.AreEqual(
            Path.Combine("repo", ".tools", "wt", "umbraco", "Data", "Umbraco_log.ldf"),
            SqlServerDatabaseCreator.GetLogFileName(dataFileName));
    }

    [Test]
    public void Can_Derive_The_Log_File_Beside_The_Data_File_When_The_Path_Contains_A_Parent_Segment()
    {
        // SQL Server's own derivation consumes one segment too many here, resolving the log of
        // "repo\a\..\b\Umbraco.mdf" to "repo\Umbraco_log.ldf" rather than "repo\b\Umbraco_log.ldf".
        // The rule the derivation has to uphold is that the log sits in the same directory as the
        // data file, whatever dot segments the path happens to contain.
        var dataFileName = Path.Combine("repo", "a", "..", "b", "Umbraco.mdf");

        var logFileName = SqlServerDatabaseCreator.GetLogFileName(dataFileName);

        Assert.AreEqual(
            Path.GetDirectoryName(Path.GetFullPath(dataFileName)),
            Path.GetDirectoryName(Path.GetFullPath(logFileName)));
    }

    [Test]
    public void Can_Derive_The_Log_File_When_The_Data_File_Has_No_Directory()
        => Assert.AreEqual("Umbraco_log.ldf", SqlServerDatabaseCreator.GetLogFileName("Umbraco.mdf"));

    [Test]
    public void Can_Derive_A_Log_File_Whose_Name_Matches_The_Log_Name()
    {
        // The two are used together in a single CREATE DATABASE statement, so they must agree.
        var dataFileName = Path.Combine("umbraco", "Data", "My.Site.mdf");

        Assert.AreEqual(
            SqlServerDatabaseCreator.GetLogName(dataFileName) + ".ldf",
            Path.GetFileName(SqlServerDatabaseCreator.GetLogFileName(dataFileName)));
    }
}
