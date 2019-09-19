using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering the DataTypeService with cache enabled
    /// </summary>
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class CachedDataTypeServiceTests : BaseServiceTest
    {
        protected override CacheHelper CreateCacheHelper()
        {
            return new CacheHelper(
                new ObjectCacheRuntimeCacheProvider(),
                new StaticCacheProvider(),
                new NullCacheProvider(),
                new IsolatedRuntimeCache(type => new ObjectCacheRuntimeCacheProvider()));
        }

        /// <summary>
        /// This tests validates that with the new scope changes that the underlying cache policies work - in this case it tests that the cache policy
        /// with Count verification works.
        /// </summary>
        [Test]
        public void DataTypeService_Can_Get_All()
        {
            var dataTypeService = ServiceContext.DataTypeService;

            IDataTypeDefinition dataTypeDefinition = new DataTypeDefinition(-1, "Test.TestEditor") { Name = "Testing Textfield", DatabaseType = DataTypeDatabaseType.Ntext };
            dataTypeService.Save(dataTypeDefinition);

            //Get all the first time (no cache)
            var all = dataTypeService.GetAllDataTypeDefinitions();
            //Get all a second time (with cache)
            all = dataTypeService.GetAllDataTypeDefinitions();

            Assert.Pass();
        }
    }
}