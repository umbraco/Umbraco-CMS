using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using UmbracoExamine.DataServices;

namespace Umbraco.Tests.UmbracoExamine
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
    [TestFixture]
    public class ContentServiceTest : BaseDatabaseFactoryTest
    {
        
        [Test]
        public void Get_All_User_Property_Names()
        {
            var contentService = new UmbracoContentService(ApplicationContext);
            var db = DatabaseContext.Database;

            var result = contentService.GetAllUserPropertyNames();

            Assert.IsTrue(result.Select(x => x).ContainsAll(new[] { "contents", Constants.Conventions.Media.Bytes, Constants.Conventions.Media.Extension, Constants.Conventions.Media.File, Constants.Conventions.Media.Height, Constants.Conventions.Media.Width }));
        }

    }
}