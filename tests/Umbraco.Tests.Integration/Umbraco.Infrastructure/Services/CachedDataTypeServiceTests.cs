// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering the DataTypeService with cache enabled
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class CachedDataTypeServiceTests : UmbracoIntegrationTest
{
    private IDataValueEditorFactory DataValueEditorFactory => GetRequiredService<IDataValueEditorFactory>();
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

    /// <summary>
    ///     This tests validates that with the new scope changes that the underlying cache policies work - in this case it
    ///     tests that the cache policy
    ///     with Count verification works.
    /// </summary>
    [Test]
    public void DataTypeService_Can_Get_All()
    {
        IDataType dataType =
            new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = "Testing Textfield",
                DatabaseType = ValueStorageType.Ntext
            };
        DataTypeService.Save(dataType);

        // Get all the first time (no cache)
        var all = DataTypeService.GetAll();

        // Get all a second time (with cache)
        all = DataTypeService.GetAll();

        Assert.Pass();
    }
}
