using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
    [TestFixture, RequiresSTA]
    public class RelationServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Can_Create_RelationType_Without_Name()
        {
            var rs = ServiceContext.RelationService;
            var rt = new RelationType(new Guid(Constants.ObjectTypes.Document), new Guid(Constants.ObjectTypes.Document), "repeatedEventOccurence");

            Assert.DoesNotThrow(() => rs.Save(rt));

            Assert.AreEqual(rt.Name, "repeatedEventOccurence");
        }
    }
}