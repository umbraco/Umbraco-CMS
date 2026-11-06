// Copyright (c) Umbraco.
// See LICENSE for more details.

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

    [TestCase("", SqlServerSyntaxProvider.VersionName.Invalid)]
    [TestCase("13.0.7050.2", SqlServerSyntaxProvider.VersionName.V2016)]
    [TestCase("14.0.3485.1", SqlServerSyntaxProvider.VersionName.V2017)]
    [TestCase("15.0.4430.1", SqlServerSyntaxProvider.VersionName.V2019)]
    [TestCase("16.0.4195.2", SqlServerSyntaxProvider.VersionName.V2022)]
    [TestCase("17.0.1000.7", SqlServerSyntaxProvider.VersionName.V2025)]
    [TestCase("18.0.100.1", SqlServerSyntaxProvider.VersionName.Other)]
    public void Can_Map_Product_Version_To_Version_Name(string productVersion, SqlServerSyntaxProvider.VersionName expected)
    {
        var result = new SqlServerSyntaxProvider.ServerVersionInfo(
            "Developer Edition (64-bit)",
            "MSSQLSERVER",
            productVersion,
            SqlServerSyntaxProvider.EngineEdition.Enterprise,
            "localhost",
            "RTM");

        Assert.That(result.ProductVersionName, Is.EqualTo(expected));
    }
}
