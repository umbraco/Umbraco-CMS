// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AuditServiceTests : UmbracoIntegrationTest
{
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
    public async Task WriteAsync_and_GetAll()
    {
        var sut = (AuditService)Services.GetRequiredService<IAuditService>();
        var expected = new AuditEntryBuilder().Build();

        for (var i = 0; i < 3; i++)
        {
            var actual = await sut.WriteAsync(
                expected.PerformingUserId + i,
                expected.PerformingDetails!,
                expected.PerformingIp!,
                expected.EventDateUtc,
                expected.AffectedUserId,
                expected.AffectedDetails,
                expected.EventType!,
                expected.EventDetails!);

            Assert.IsTrue(actual.Success);
            Assert.AreEqual(AuditLogOperationStatus.Success, actual.Status);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected.PerformingUserId + i, actual.Result.PerformingUserId);
                Assert.AreEqual(expected.PerformingDetails, actual.Result.PerformingDetails);
                Assert.AreEqual(expected.EventDateUtc, actual.Result.EventDateUtc);
                Assert.AreEqual(expected.AffectedUserId, actual.Result.AffectedUserId);
                Assert.AreEqual(expected.AffectedDetails, actual.Result.AffectedDetails);
                Assert.AreEqual(expected.EventType, actual.Result.EventType);
                Assert.AreEqual(expected.EventDetails, actual.Result.EventDetails);
            });
        }

        var entries = sut.GetAll().ToArray();

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(entries);
            Assert.AreEqual(3, entries.Length);
            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(expected.PerformingUserId + i, entries[i].PerformingUserId);
            }
        });
    }

    [Test]
    public void Write_and_GetAll()
    {
        var sut = (AuditService)Services.GetRequiredService<IAuditService>();
        var expected = new AuditEntryBuilder().Build();

        for (var i = 0; i < 3; i++)
        {
            var actual = sut.Write(
                expected.PerformingUserId + i,
                expected.PerformingDetails!,
                expected.PerformingIp!,
                expected.EventDateUtc,
                expected.AffectedUserId,
                expected.AffectedDetails,
                expected.EventType!,
                expected.EventDetails!);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected.PerformingUserId + i, actual.PerformingUserId);
                Assert.AreEqual(expected.PerformingDetails, actual.PerformingDetails);
                Assert.AreEqual(expected.EventDateUtc, actual.EventDateUtc);
                Assert.AreEqual(expected.AffectedUserId, actual.AffectedUserId);
                Assert.AreEqual(expected.AffectedDetails, actual.AffectedDetails);
                Assert.AreEqual(expected.EventType, actual.EventType);
                Assert.AreEqual(expected.EventDetails, actual.EventDetails);
            });
        }

        var entries = sut.GetAll().ToArray();

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(entries);
            Assert.AreEqual(3, entries.Length);
            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(expected.PerformingUserId + i, entries[i].PerformingUserId);
            }
        });
    }
}
