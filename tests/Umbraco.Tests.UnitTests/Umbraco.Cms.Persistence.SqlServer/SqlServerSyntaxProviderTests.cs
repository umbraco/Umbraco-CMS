// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Persistence.SqlServer.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.SqlServer;

[TestFixture]
public class SqlServerSyntaxProviderTests
{
    [Test]
    public void Can_Format_Guid_Unchanged()
    {
        var sut = new SqlServerSyntaxProvider(Options.Create(new GlobalSettings()));

        var result = sut.FormatGuid(new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));

        Assert.That(result, Is.EqualTo("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));
    }

    [Test]
    public void AliasRegex_Captures_Qualified_Column_And_Alias_Separately()
    {
        var sut = new SqlServerSyntaxProvider(Options.Create(new GlobalSettings()));

        const string sql = "SELECT [table].[column1] AS [alias1], [table].[column2] AS [alias2] FROM [table];";
        MatchCollection matches = sut.AliasRegex.Matches(sql);

        Assert.That(sut.AliasRegex.ToString(), Is.EqualTo(@"(\[\w+]\.\[\w+])\s+AS\s+(\[\w+])"));
        Assert.That(matches, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(matches[0].Groups[1].Value, Is.EqualTo("[table].[column1]"));
            Assert.That(matches[0].Groups[2].Value, Is.EqualTo("[alias1]"));
            Assert.That(matches[1].Groups[1].Value, Is.EqualTo("[table].[column2]"));
            Assert.That(matches[1].Groups[2].Value, Is.EqualTo("[alias2]"));
        });
    }
}
