using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class AuditServiceTests : TestWithDatabaseBase
    {
        [Test]
        public void CanCrudAuditEntry()
        {
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var entry = ServiceContext.AuditService.Write(123, "user 123, bob@example.com", null, yesterday, 456, "user 456, alice@example.com", "umbraco/user", "change property whatever value");
            Assert.AreEqual(123, entry.PerformingUserId);
            Assert.AreEqual("user 123, bob@example.com", entry.PerformingDetails);
            Assert.AreEqual(yesterday, entry.EventDateUtc);
            Assert.AreEqual(456, entry.AffectedUserId);
            Assert.AreEqual("user 456, alice@example.com", entry.AffectedDetails);
            Assert.AreEqual("umbraco/user", entry.EventType);
            Assert.AreEqual("change property whatever value", entry.EventDetails);

            var entries = ((AuditService)ServiceContext.AuditService).GetAll().ToArray();
            Assert.IsNotNull(entries);
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual(123, entries[0].PerformingUserId);

            for (var i = 0; i < 10; i++)
            {
                yesterday = yesterday.AddMinutes(1);
                entry = ServiceContext.AuditService.Write(123 + i, "user 123, bob@example.com", null, yesterday, 456 + i, "user 456, alice@example.com", "umbraco/user", "change property whatever value");
            }

            //
            // page 0 contains 123+9, 123+8
            // page 1 contains 123+7, 123+6
            // page 2 contains 123+5, 123+4
            // ...

            entries = ((AuditService)ServiceContext.AuditService).GetPage(2, 2, out var count).ToArray();

            Assert.AreEqual(2, entries.Length);

            Assert.AreEqual(123 + 5, entries[0].PerformingUserId);
            Assert.AreEqual(123 + 4, entries[1].PerformingUserId);
        }
    }
}
