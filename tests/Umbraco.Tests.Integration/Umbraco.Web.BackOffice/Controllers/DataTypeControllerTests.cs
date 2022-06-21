using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Formatters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.BackOffice.Controllers;

[TestFixture]
public class DataTypeControllerTests : UmbracoTestServerTestBase
{
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task Has_Values_Returns_Correct_Values(bool expectHasValues)
    {
        var dataTypeService = GetRequiredService<IDataTypeService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();
        var contentService = GetRequiredService<IContentService>();
        var serializer = GetRequiredService<IJsonSerializer>();

        var dataType = new DataTypeBuilder()
            .WithId(0)
            .WithoutIdentity()
            .WithDatabaseType(ValueStorageType.Ntext)
            .Build();

        dataTypeService.Save(dataType);

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .AddPropertyType()
            .WithDataTypeId(dataType.Id)
            .WithName("Test")
            .WithAlias("test")
            .Done()
            .WithContentVariation(ContentVariation.Nothing)
            .Build();

        contentTypeService.Save(contentType);

        if (expectHasValues)
        {
            var content = new ContentBuilder()
                .WithId(0)
                .WithContentType(contentType)
                .AddPropertyData()
                .WithKeyValue("test", "Some Value")
                .Done()
                .Build();

            contentService.Save(content);
        }

        var url = PrepareApiControllerUrl<DataTypeController>(x => x.HasValues(dataType.Id));

        var response = await Client.GetAsync(url);

        var body = await response.Content.ReadAsStringAsync();
        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);

        var result = serializer.Deserialize<DataTypeHasValuesDisplay>(body);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(dataType.Id, result.Id);
            Assert.AreEqual(expectHasValues, result.HasValues);
        });
    }
}
