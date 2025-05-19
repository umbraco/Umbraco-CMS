// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AuditServiceTests : UmbracoIntegrationTest
{
    [Test]
    public async Task GetUserLogs()
    {
        var sut = (AuditService)Services.GetRequiredService<IAuditService>();

        var eventDateUtc = DateTime.UtcNow.AddDays(-1);

        const int numberOfEntries = 10;
        for (var i = 0; i < numberOfEntries; i++)
        {
            eventDateUtc = eventDateUtc.AddMinutes(1);
            await sut.AddAsync(AuditType.Unpublish, Constants.Security.SuperUserKey, 33, string.Empty, "blah");
        }

        await sut.AddAsync(AuditType.Publish, Constants.Security.SuperUserKey, 33, string.Empty, "blah");

        var logs = (await sut.GetPagedItemsByUserAsync(
            Constants.Security.SuperUserKey,
            0,
            int.MaxValue,
            auditTypeFilter: [AuditType.Unpublish])).Items.ToArray();

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(logs);
            CollectionAssert.AllItemsAreNotNull(logs);
            Assert.AreEqual(numberOfEntries, logs.Length);
            Assert.AreEqual(numberOfEntries, logs.Count(x => x.AuditType == AuditType.Unpublish));
        });
    }
}
