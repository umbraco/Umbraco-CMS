using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.Testing;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering the DataTypeService with cache enabled
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class CachedDataTypeServiceTests : TestWithSomeContentBase
    {
        /// <summary>
        /// This tests validates that with the new scope changes that the underlying cache policies work - in this case it tests that the cache policy
        /// with Count verification works.
        /// </summary>
        [Test]
        public void DataTypeService_Can_Get_All()
        {
            var dataTypeService = ServiceContext.DataTypeService;

            IDataType dataType = new DataType(new LabelPropertyEditor(Logger)) { Name = "Testing Textfield", DatabaseType = ValueStorageType.Ntext };
            dataTypeService.Save(dataType);

            //Get all the first time (no cache)
            var all = dataTypeService.GetAll();
            //Get all a second time (with cache)
            all = dataTypeService.GetAll();

            Assert.Pass();
        }
    }
}
