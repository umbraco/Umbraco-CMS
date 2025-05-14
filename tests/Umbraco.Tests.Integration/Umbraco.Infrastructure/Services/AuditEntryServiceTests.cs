// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AuditEntryServiceTests : UmbracoIntegrationTest
{
    [Test]
    public async Task Write_and_GetAll()
    {
        var sut = (AuditEntryService)Services.GetRequiredService<IAuditEntryService>();
        var expected = new AuditEntryBuilder().Build();

        var result = await sut.WriteAsync(
            expected.PerformingUserId,
            expected.PerformingDetails,
            expected.PerformingIp,
            expected.EventDateUtc,
            expected.AffectedUserId,
            expected.AffectedDetails,
            expected.EventType,
            expected.EventDetails);
        Assert.IsTrue(result.Success);

        var actual = result.Result;

        var entries = sut.GetAll().ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(expected.PerformingUserId, actual.PerformingUserId);
            Assert.AreEqual(expected.PerformingDetails, actual.PerformingDetails);
            Assert.AreEqual(expected.EventDateUtc, actual.EventDateUtc);
            Assert.AreEqual(expected.AffectedUserId, actual.AffectedUserId);
            Assert.AreEqual(expected.AffectedDetails, actual.AffectedDetails);
            Assert.AreEqual(expected.EventType, actual.EventType);
            Assert.AreEqual(expected.EventDetails, actual.EventDetails);
            Assert.IsNotNull(entries);
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual(expected.PerformingUserId, entries[0].PerformingUserId);
        });
    }
}
