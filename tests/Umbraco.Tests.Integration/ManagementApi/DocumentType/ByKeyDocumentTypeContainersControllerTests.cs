using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType;

public class ByKeyDocumentTypeContainersControllerTests : ManagementApiTest<ByKeyDocumentTypeController>
{
    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private Guid _key;

    protected override Expression<Func<ByKeyDocumentTypeController, object>> MethodSelector
    {
        get => x => x.ByKey(CancellationToken.None, _key);
        set => throw new NotSupportedException();
    }

    [TestCase("Content", "Meta")]
    [TestCase("Content/Settings", "Meta/Data")]
    [TestCase("#Technical data - Display", "Group: One")]
    public async Task Container_Keeps_Its_Parent_Regardless_Of_Names(string tabName, string groupName)
    {
        _key = Guid.NewGuid();
        var tab = new ContentTypePropertyContainerModel { Key = Guid.NewGuid(), Name = tabName, Type = "Tab" };
        var group = new ContentTypePropertyContainerModel { Key = Guid.NewGuid(), Name = groupName, Type = "Group", ParentKey = tab.Key };
        var createModel = new ContentTypeCreateModel
        {
            Key = _key,
            Name = _key.ToString(),
            Alias = $"test{_key:N}",
            Containers = [tab, group],
            Properties =
            [
                new ContentTypePropertyTypeModel
                {
                    Name = "Title",
                    Alias = "title",
                    DataTypeKey = Constants.DataTypes.Guids.TextstringGuid,
                    ContainerKey = group.Key,
                },
            ],
        };
        var createResult = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success, createResult.Status.ToString());

        await AuthenticateClientAsync(Client, "admin@umbraco.com", UserPassword, isAdmin: true);
        HttpResponseMessage response = await Client.GetAsync(Url);
        var body = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, body);

        using JsonDocument document = JsonDocument.Parse(body);
        JsonElement[] containers = document.RootElement.GetProperty("containers").EnumerateArray().ToArray();
        JsonElement groupContainer = containers.Single(c => c.GetProperty("id").GetGuid() == group.Key);
        JsonElement tabContainer = containers.Single(c => c.GetProperty("id").GetGuid() == tab.Key);

        Assert.AreEqual(JsonValueKind.Null, tabContainer.GetProperty("parent").ValueKind);
        JsonElement groupParent = groupContainer.GetProperty("parent");
        Assert.AreEqual(JsonValueKind.Object, groupParent.ValueKind);
        Assert.AreEqual(tab.Key, groupParent.GetProperty("id").GetGuid());
    }
}
