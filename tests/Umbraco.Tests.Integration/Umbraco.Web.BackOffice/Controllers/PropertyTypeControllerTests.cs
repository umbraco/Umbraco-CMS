using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Cms.Core;
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
public class PropertyTypeControllerTests : UmbracoTestServerTestBase
{
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task Has_Values_Returns_Correct_Values(bool expectHasValues)
    {
        IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();
        IContentService contentService = GetRequiredService<IContentService>();
        IJsonSerializer serializer = GetRequiredService<IJsonSerializer>();

        string propertyTypeAlias = "title";
        IContentType contentType = new ContentTypeBuilder()
            .WithId(0)
            .AddPropertyType()
            .WithAlias(propertyTypeAlias)
            .WithValueStorageType(ValueStorageType.Integer)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithName("Title")
            .Done()
            .WithContentVariation(ContentVariation.Nothing)
            .Build();

        contentTypeService.Save(contentType);

        if (expectHasValues)
        {
            Content content = new ContentBuilder()
                .WithId(0)
                .WithName("TestContent")
                .WithContentType(contentType)
                .AddPropertyData()
                .WithKeyValue("title", "someValue")
                .Done()
                .Build();

            contentService.Save(content);
        }

        string url = PrepareApiControllerUrl<PropertyTypeController>(x => x.HasValues(propertyTypeAlias));

        HttpResponseMessage response = await Client.GetAsync(url);

        string body = await response.Content.ReadAsStringAsync();
        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);

        var result = serializer.Deserialize<PropertyTypeHasValuesDisplay>(body);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(propertyTypeAlias, result.PropertyTypeAlias);
            Assert.AreEqual(expectHasValues, result.HasValues);
        });

    }
}
