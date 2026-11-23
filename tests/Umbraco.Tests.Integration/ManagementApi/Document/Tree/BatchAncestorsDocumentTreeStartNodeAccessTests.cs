using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document.Tree;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document.Tree;

/// <summary>
/// Asserts that the bulk document tree ancestors endpoint applies the user's content start nodes, unless the
/// requesting data type ignores them, and the user's browse permissions.
/// </summary>
public class BatchAncestorsDocumentTreeStartNodeAccessTests : StartNodeAccessTestBase<BatchAncestorsDocumentTreeController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private JsonSerializerOptions BackOfficeJsonSerializerOptions => GetRequiredService<IOptionsMonitor<JsonOptions>>()
        .Get(Constants.JsonOptionsNames.BackOffice)
        .JsonSerializerOptions;

    // siteRoot > startNode > insideChild > insideGrandchild, with the user's start node at startNode.
    private IContent _siteRoot = null!;
    private IContent _startNode = null!;
    private IContent _insideChild = null!;
    private IContent _insideGrandchild = null!;

    // startNode > unbrowsable > browsable, where the user cannot browse unbrowsable but can browse browsable.
    private IContent _unbrowsable = null!;
    private IContent _browsable = null!;

    // outsideRoot > outsideChild, outside the user's start nodes.
    private IContent _outsideRoot = null!;
    private IContent _outsideChild = null!;

    protected override Expression<Func<BatchAncestorsDocumentTreeController, object>> MethodSelector { get; set; } =
        x => x.AncestorsBatch(CancellationToken.None, new HashSet<Guid>(), null);

    [SetUp]
    public async Task SetUpDocumentsAndUser()
    {
        IContentType contentType = ContentTypeBuilder.CreateBasicContentType(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        _siteRoot = await CreateDocumentAsync(contentType.Key, Constants.System.RootKey);
        _startNode = await CreateDocumentAsync(contentType.Key, _siteRoot.Key);
        _insideChild = await CreateDocumentAsync(contentType.Key, _startNode.Key);
        _insideGrandchild = await CreateDocumentAsync(contentType.Key, _insideChild.Key);
        _unbrowsable = await CreateDocumentAsync(contentType.Key, _startNode.Key);
        _browsable = await CreateDocumentAsync(contentType.Key, _unbrowsable.Key);

        _outsideRoot = await CreateDocumentAsync(contentType.Key, Constants.System.RootKey);
        _outsideChild = await CreateDocumentAsync(contentType.Key, _outsideRoot.Key);

        await AuthenticateRestrictedUserAsync(
            [Constants.Applications.Content],
            startContentId: _startNode.Id,
            granularPermissions:
            [
                new DocumentGranularPermission { Key = _unbrowsable.Key, Permission = ActionUpdate.ActionLetter },
                new DocumentGranularPermission { Key = _browsable.Key, Permission = ActionBrowse.ActionLetter },
            ]);
    }

    [Test]
    public async Task Omits_Ancestors_The_User_Cannot_Browse()
    {
        BatchResponseModel<DocumentTreeAncestorsResponseModel> result = await GetAncestorsAsync([_browsable.Key]);

        CollectionAssert.AreEqual(
            new[] { _siteRoot.Key, _startNode.Key },
            result.Items.Single().Ancestors.Select(ancestor => ancestor.Id));
    }

    [Test]
    public async Task Omits_Items_The_User_Cannot_Browse()
    {
        BatchResponseModel<DocumentTreeAncestorsResponseModel> result =
            await GetAncestorsAsync([_unbrowsable.Key, _insideChild.Key]);

        CollectionAssert.AreEqual(new[] { _insideChild.Key }, result.Items.Select(chain => chain.Id));
    }

    [Test]
    public async Task Returns_Ancestors_Ordered_From_The_Root_Down_Excluding_The_Item()
    {
        BatchResponseModel<DocumentTreeAncestorsResponseModel> result = await GetAncestorsAsync([_insideGrandchild.Key]);

        DocumentTreeAncestorsResponseModel chain = result.Items.Single();
        Assert.AreEqual(_insideGrandchild.Key, chain.Id);
        CollectionAssert.AreEqual(
            new[] { _siteRoot.Key, _startNode.Key, _insideChild.Key },
            chain.Ancestors.Select(ancestor => ancestor.Id));
    }

    [Test]
    public async Task Marks_Ancestors_Above_The_User_Start_Node_As_No_Access()
    {
        BatchResponseModel<DocumentTreeAncestorsResponseModel> result = await GetAncestorsAsync([_insideGrandchild.Key]);

        CollectionAssert.AreEqual(
            new[] { true, false, false },
            result.Items.Single().Ancestors.Select(ancestor => ancestor.NoAccess));
    }

    [Test]
    public async Task Returns_No_Ancestors_For_A_Root_Level_Item()
    {
        BatchResponseModel<DocumentTreeAncestorsResponseModel> result = await GetAncestorsAsync([_siteRoot.Key]);

        DocumentTreeAncestorsResponseModel chain = result.Items.Single();
        Assert.AreEqual(_siteRoot.Key, chain.Id);
        Assert.IsEmpty(chain.Ancestors);
    }

    [Test]
    public async Task Returns_Items_In_The_Requested_Order_Omitting_Unknown_Items()
    {
        BatchResponseModel<DocumentTreeAncestorsResponseModel> result =
            await GetAncestorsAsync([_insideGrandchild.Key, Guid.NewGuid(), _insideChild.Key]);

        Assert.AreEqual(2, result.Total);
        CollectionAssert.AreEqual(
            new[] { _insideGrandchild.Key, _insideChild.Key },
            result.Items.Select(chain => chain.Id));
    }

    [Test]
    public async Task Omits_Items_Outside_The_User_Start_Nodes()
    {
        BatchResponseModel<DocumentTreeAncestorsResponseModel> result =
            await GetAncestorsAsync([_outsideChild.Key, _insideChild.Key]);

        CollectionAssert.AreEqual(new[] { _insideChild.Key }, result.Items.Select(chain => chain.Id));
    }

    [Test]
    public async Task Returns_Ancestors_Outside_The_User_Start_Nodes_For_A_Data_Type_Ignoring_Start_Nodes()
    {
        IDataType dataType = await CreatePickerDataTypeAsync(ignoreUserStartNodes: true);

        BatchResponseModel<DocumentTreeAncestorsResponseModel> result =
            await GetAncestorsAsync([_outsideChild.Key], dataType.Key);

        DocumentTreeAncestorsResponseModel chain = result.Items.Single();
        Assert.AreEqual(_outsideChild.Key, chain.Id);
        DocumentTreeItemResponseModel ancestor = chain.Ancestors.Single();
        Assert.AreEqual(_outsideRoot.Key, ancestor.Id);
        Assert.IsFalse(ancestor.NoAccess);
    }

    [Test]
    public async Task Applies_The_User_Start_Nodes_For_A_Data_Type_Not_Ignoring_Start_Nodes()
    {
        IDataType dataType = await CreatePickerDataTypeAsync(ignoreUserStartNodes: false);

        BatchResponseModel<DocumentTreeAncestorsResponseModel> result =
            await GetAncestorsAsync([_outsideChild.Key], dataType.Key);

        Assert.IsEmpty(result.Items);
    }

    private async Task<BatchResponseModel<DocumentTreeAncestorsResponseModel>> GetAncestorsAsync(
        IEnumerable<Guid> keys,
        Guid? dataTypeKey = null)
    {
        var url = WithIds(Url, keys);
        if (dataTypeKey is not null)
        {
            url += $"&dataTypeId={dataTypeKey}";
        }

        HttpResponseMessage response = await Client.GetAsync(url);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        BatchResponseModel<DocumentTreeAncestorsResponseModel>? result =
            await response.Content.ReadFromJsonAsync<BatchResponseModel<DocumentTreeAncestorsResponseModel>>(BackOfficeJsonSerializerOptions);
        Assert.IsNotNull(result);
        return result!;
    }

    private async Task<IDataType> CreatePickerDataTypeAsync(bool ignoreUserStartNodes)
    {
        IDataEditor editor = PropertyEditorCollection[Constants.PropertyEditors.Aliases.MultiNodeTreePicker]
                             ?? throw new InvalidOperationException("The multi-node tree picker property editor was not registered.");

        var dataType = new Core.Models.DataType(editor, ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                { "startNode", new MultiNodePickerConfigurationTreeSource { ObjectType = "content" } },
                { Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes, ignoreUserStartNodes },
            },
            Name = Guid.NewGuid().ToString(),
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };

        Attempt<IDataType, DataTypeOperationStatus> attempt =
            await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(attempt.Success, $"Could not create the data type: {attempt.Status}");

        return attempt.Result;
    }

    private async Task<IContent> CreateDocumentAsync(Guid contentTypeKey, Guid? parentKey)
    {
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentTypeKey,
            ParentKey = parentKey,
            Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
        };

        Attempt<ContentCreateResult, ContentEditingOperationStatus> attempt =
            await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(attempt.Success, $"Could not create the document: {attempt.Status}");

        return attempt.Result.Content!;
    }
}
