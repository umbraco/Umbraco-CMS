using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Template;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing.Document;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentTypeEditingServiceTests
{

    [Test]
    public async Task Can_Create_Basic_DocumentType()
    {
        var name = "Test";
        var alias = "test";

        var createModel = new DocumentTypeCreateModel { Alias = alias, Name = name };
        createModel.IsElement = true;

        var response = await DocumentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // Ensure it's actually persisted
        var documentType = await ContentTypeService.GetAsync(response.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(documentType);
            Assert.AreEqual(alias, documentType.Alias);
            Assert.AreEqual(name, documentType.Name);
            Assert.AreEqual(response.Result.Id, documentType.Id);
            Assert.AreEqual(response.Result.Key, documentType.Key);
        });
    }
}
