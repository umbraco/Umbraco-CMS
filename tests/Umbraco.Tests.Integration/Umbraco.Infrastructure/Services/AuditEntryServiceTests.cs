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
        Assert.That(result, Is.Not.Null);

        var actual = result;

        var entries = sut.GetAll().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(actual.PerformingUserId, Is.EqualTo(expected.PerformingUserId));
            Assert.That(actual.PerformingUserKey, Is.EqualTo(expected.PerformingUserKey));
            Assert.That(actual.PerformingDetails, Is.EqualTo(expected.PerformingDetails));
            Assert.That(actual.EventDate, Is.EqualTo(expected.EventDate));
            Assert.That(actual.AffectedUserId, Is.EqualTo(expected.AffectedUserId));
            Assert.That(actual.AffectedUserKey, Is.EqualTo(expected.AffectedUserKey));
            Assert.That(actual.AffectedDetails, Is.EqualTo(expected.AffectedDetails));
            Assert.That(actual.EventType, Is.EqualTo(expected.EventType));
            Assert.That(actual.EventDetails, Is.EqualTo(expected.EventDetails));
            Assert.That(entries, Is.Not.Null);
            Assert.That(entries, Has.Length.EqualTo(1));
            Assert.That(entries[0].PerformingUserKey, Is.EqualTo(expected.PerformingUserKey));
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
        Assert.That(result, Is.Not.Null);

        var actual = result;

        var entries = sut.GetAll().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(actual.PerformingUserId, Is.EqualTo(Constants.Security.SuperUserId));
            Assert.That(actual.PerformingUserKey, Is.EqualTo(Constants.Security.SuperUserKey));
            Assert.That(actual.PerformingDetails, Is.EqualTo("performingDetails"));
            Assert.That(actual.PerformingIp, Is.EqualTo("performingIp"));
            Assert.That(actual.EventDate, Is.EqualTo(eventDateUtc));
            Assert.That(actual.AffectedUserId, Is.EqualTo(Constants.Security.UnknownUserId));
            Assert.That(actual.AffectedUserKey, Is.EqualTo(null));
            Assert.That(actual.AffectedDetails, Is.EqualTo("affectedDetails"));
            Assert.That(actual.EventType, Is.EqualTo("umbraco/test"));
            Assert.That(actual.EventDetails, Is.EqualTo("eventDetails"));
            Assert.That(entries, Is.Not.Null);
            Assert.That(entries, Has.Length.EqualTo(1));
            Assert.That(entries[0].PerformingUserId, Is.EqualTo(Constants.Security.SuperUserId));
        });
    }
}
