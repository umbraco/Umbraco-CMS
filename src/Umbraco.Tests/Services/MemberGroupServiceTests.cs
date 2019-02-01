using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class MemberGroupServiceTests : BaseServiceTest
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
        [ExpectedException("System.InvalidOperationException")]
        public void New_MemberGroup_Is_Not_Allowed_With_Empty_Name()
        {
            var service = ServiceContext.MemberGroupService;
            
            service.Save(new MemberGroup {Name = ""});

            Assert.Fail("An exception should have been thrown");
        }
    }
}
