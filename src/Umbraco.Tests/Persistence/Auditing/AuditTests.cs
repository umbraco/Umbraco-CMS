using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Auditing;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Auditing
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
    [TestFixture]
    public class AuditTests : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [Test]
        public void Can_Add_Audit_Entry()
        {
            Audit.Add(AuditTypes.System, "This is a System audit trail", 0, -1);

            var dtos = DatabaseContext.Database.Fetch<LogDto>("WHERE id > -1");

            Assert.That(dtos.Any(), Is.True);
            Assert.That(dtos.First().Comment, Is.EqualTo("This is a System audit trail"));
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }
    }
}