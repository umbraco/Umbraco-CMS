// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Persistence.Sqlite.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.Sqlite;

[TestFixture]
public class SqliteSyntaxProviderTests
{
    [Test]
    public void Can_Format_Guid_Uppercase()
    {
        var sut = new SqliteSyntaxProvider(
            Options.Create(new GlobalSettings()),
            Mock.Of<ILogger<SqliteSyntaxProvider>>());

        var result = sut.FormatGuid(new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));

        Assert.That(result, Is.EqualTo("A1B2C3D4-E5F6-7890-ABCD-EF1234567890"));
    }
}
