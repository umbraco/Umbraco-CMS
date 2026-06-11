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
}
