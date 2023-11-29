using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType.Items;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType.Items;

/// <summary>
///
/// </summary>
[TestFixture]
public class ItemDatatypeItemControllerTests : ManagementApiTest<ItemDatatypeItemController>
{
    protected override Expression<Func<ItemDatatypeItemController, object>> MethodSelector =>
        x => x.Item(new HashSet<Guid> { Guid.NewGuid() });

    [Test]
    public virtual async Task As_Admin_I_Have_Access()
    {
        await AuthenticateClientAsync(Client, "admin@umbraco.com", "1234567890", Constants.Security.AdminGroupKey);

        var response = await Client.GetAsync(Url);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Editor_I_Have_Access()
    {
        await AuthenticateClientAsync(Client, "editor@umbraco.com", "1234567890", Constants.Security.EditorGroupKey);

        var response = await Client.GetAsync(Url);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Sensitive_Data_I_Have_No_Access()
    {
        await AuthenticateClientAsync(Client, "sensitiveData@umbraco.com", "1234567890", Constants.Security.SensitiveDataGroupKey);

        var response = await Client.GetAsync(Url);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Translator_I_Have_No_Access()
    {
        await AuthenticateClientAsync(Client, "translator@umbraco.com", "1234567890", Constants.Security.TranslatorGroupKey);

        var response = await Client.GetAsync(Url);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Writer_I_Have_Access()
    {
        await AuthenticateClientAsync(Client, "writer@umbraco.com", "1234567890", Constants.Security.WriterGroupKey);

        var response = await Client.GetAsync(Url);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }


    [Test]
    public virtual async Task Unauthorized_When_No_Token_Is_Provided()
    {
        var response = await Client.GetAsync(Url);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}
