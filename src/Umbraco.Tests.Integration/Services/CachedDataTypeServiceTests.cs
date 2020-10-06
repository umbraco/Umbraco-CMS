using System.Threading;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering the DataTypeService with cache enabled
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class CachedDataTypeServiceTests : UmbracoIntegrationTest
    {
        private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();
        private ILocalizedTextService LocalizedTextService => GetRequiredService<ILocalizedTextService>();
        private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

        /// <summary>
        /// This tests validates that with the new scope changes that the underlying cache policies work - in this case it tests that the cache policy
        /// with Count verification works.
        /// </summary>
        [Test]
        public void DataTypeService_Can_Get_All()
        {
            var dataTypeService = GetRequiredService<IDataTypeService>();

            IDataType dataType = new DataType(new LabelPropertyEditor(Logger, IOHelper, DataTypeService, LocalizedTextService, LocalizationService, ShortStringHelper)) { Name = "Testing Textfield", DatabaseType = ValueStorageType.Ntext };
            dataTypeService.Save(dataType);

            //Get all the first time (no cache)
            var all = dataTypeService.GetAll();
            //Get all a second time (with cache)
            all = dataTypeService.GetAll();

            Assert.Pass();
        }
    }
}
