using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType;

public class CreateDataTypeControllerProductionModeTests : ManagementApiTest<CreateDataTypeController>
{
    protected override Expression<Func<CreateDataTypeController, object>> MethodSelector { get; set; }
        = x => x.Create(CancellationToken.None, null);

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration[Constants.Configuration.ConfigRuntimeMode] = "Production";
        InMemoryConfiguration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"] = "https://localhost";
        base.Setup();
        AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Create_Returns_Bad_Request()
    {
        CreateDataTypeRequestModel createModel = new()
        {
            Id = Guid.NewGuid(),
            Parent = null,
            Name = "TestDataType",
            EditorAlias = "Umbraco.CheckBoxList",
            EditorUiAlias = "Umb.PropertyEditorUi.CheckBoxList",
        };

        var response = await Client.PostAsync(Url, JsonContent.Create(createModel));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
