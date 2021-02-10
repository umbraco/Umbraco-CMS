// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.Integration.Testing;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Services
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

        private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

        private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

        /// <summary>
        /// This tests validates that with the new scope changes that the underlying cache policies work - in this case it tests that the cache policy
        /// with Count verification works.
        /// </summary>
        [Test]
        public void DataTypeService_Can_Get_All()
        {
            IDataType dataType = new DataType(new LabelPropertyEditor(LoggerFactory, IOHelper, DataTypeService, LocalizedTextService, LocalizationService, ShortStringHelper, JsonSerializer), ConfigurationEditorJsonSerializer) { Name = "Testing Textfield", DatabaseType = ValueStorageType.Ntext };
            DataTypeService.Save(dataType);

            // Get all the first time (no cache)
            IEnumerable<IDataType> all = DataTypeService.GetAll();

            // Get all a second time (with cache)
            all = DataTypeService.GetAll();

            Assert.Pass();
        }
    }
}
