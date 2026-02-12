// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
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
        var expected = new AuditEntryBuilder()
            .Build();

        var result = await sut.WriteAsync(
            expected.PerformingUserKey.Value,
            expected.PerformingDetails,
            expected.PerformingIp,
            expected.EventDate,
            expected.AffectedUserKey.Value,
            expected.AffectedDetails,
            expected.EventType,
            expected.EventDetails);
        Assert.NotNull(result);

        var actual = result;

        var entries = sut.GetAll().ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(expected.PerformingUserId, actual.PerformingUserId);
            Assert.AreEqual(expected.PerformingUserKey, actual.PerformingUserKey);
            Assert.AreEqual(expected.PerformingDetails, actual.PerformingDetails);
            Assert.AreEqual(expected.EventDate, actual.EventDate);
            Assert.AreEqual(expected.AffectedUserId, actual.AffectedUserId);
            Assert.AreEqual(expected.AffectedUserKey, actual.AffectedUserKey);
            Assert.AreEqual(expected.AffectedDetails, actual.AffectedDetails);
            Assert.AreEqual(expected.EventType, actual.EventType);
            Assert.AreEqual(expected.EventDetails, actual.EventDetails);
            Assert.IsNotNull(entries);
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual(expected.PerformingUserKey, entries[0].PerformingUserKey);
        });
    }

    [Test]
    public async Task Write_and_GetAll_With_Keys()
    {
        var sut = (AuditEntryService)Services.GetRequiredService<IAuditEntryService>();
        var eventDateUtc = DateTime.UtcNow;
        var result = await sut.WriteAsync(
            Constants.Security.SuperUserKey,
            "performingDetails",
            "performingIp",
            eventDateUtc,
            null,
            "affectedDetails",
            "umbraco/test",
            "eventDetails");
        Assert.NotNull(result);

        var actual = result;

        var entries = sut.GetAll().ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(Constants.Security.SuperUserId, actual.PerformingUserId);
            Assert.AreEqual(Constants.Security.SuperUserKey, actual.PerformingUserKey);
            Assert.AreEqual("performingDetails", actual.PerformingDetails);
            Assert.AreEqual("performingIp", actual.PerformingIp);
            Assert.AreEqual(eventDateUtc, actual.EventDate);
            Assert.AreEqual(Constants.Security.UnknownUserId, actual.AffectedUserId);
            Assert.AreEqual(null, actual.AffectedUserKey);
            Assert.AreEqual("affectedDetails", actual.AffectedDetails);
            Assert.AreEqual("umbraco/test", actual.EventType);
            Assert.AreEqual("eventDetails", actual.EventDetails);
            Assert.IsNotNull(entries);
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual(Constants.Security.SuperUserId, entries[0].PerformingUserId);
        });
    }
}
