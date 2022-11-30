// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class AuditServiceTests : UmbracoIntegrationTest
{
    [Test]
    public void GetPage()
    {
        var sut = (AuditService)GetRequiredService<IAuditService>();
        var expected = new AuditEntryBuilder().Build();

        for (var i = 0; i < 10; i++)
        {
            sut.Write(
                expected.PerformingUserId + i,
                expected.PerformingDetails,
                expected.PerformingIp,
                expected.EventDateUtc.AddMinutes(i),
                expected.AffectedUserId + i,
                expected.AffectedDetails,
                expected.EventType,
                expected.EventDetails);
        }

        var entries = sut.GetPage(2, 2, out var count).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, entries.Length);
            Assert.AreEqual(expected.PerformingUserId + 5, entries[0].PerformingUserId);
            Assert.AreEqual(expected.PerformingUserId + 4, entries[1].PerformingUserId);
        });
    }

    [Test]
    public void GetUserLogs()
    {
        var sut = (AuditService)Services.GetRequiredService<IAuditService>();

        var eventDateUtc = DateTime.UtcNow.AddDays(-1);

        var numberOfEntries = 10;
        for (var i = 0; i < numberOfEntries; i++)
        {
            eventDateUtc = eventDateUtc.AddMinutes(1);
            sut.Add(AuditType.Unpublish, -1, 33, string.Empty, "blah");
        }

        sut.Add(AuditType.Publish, -1, 33, string.Empty, "blah");

        var logs = sut.GetUserLogs(-1, AuditType.Unpublish).ToArray();

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(logs);
            CollectionAssert.AllItemsAreNotNull(logs);
            Assert.AreEqual(numberOfEntries, logs.Length);
            Assert.AreEqual(numberOfEntries, logs.Count(x => x.AuditType == AuditType.Unpublish));
        });
    }

    [Test]
    public void Write_and_GetAll()
    {
        var sut = (AuditService)Services.GetRequiredService<IAuditService>();
        var expected = new AuditEntryBuilder().Build();

        var actual = sut.Write(
            expected.PerformingUserId,
            expected.PerformingDetails,
            expected.PerformingIp,
            expected.EventDateUtc,
            expected.AffectedUserId,
            expected.AffectedDetails,
            expected.EventType,
            expected.EventDetails);

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
