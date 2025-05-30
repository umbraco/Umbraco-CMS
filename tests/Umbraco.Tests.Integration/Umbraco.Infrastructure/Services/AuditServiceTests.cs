// Copyright (c) Umbraco.
// See LICENSE for more details.

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
}
