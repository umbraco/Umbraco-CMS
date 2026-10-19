using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public abstract class BackOfficeTestBase : TestBase
{
    private bool _fixtureIsInitialized;

    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected IContentService ContentService => GetRequiredService<IContentService>();

    protected IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    protected IMediaService MediaService => GetRequiredService<IMediaService>();

    [SetUp]
    public virtual async Task SetupTest()
    {
        // unfortunately, SetUpFixture won't work due to dependencies being setup in Umbraco test base classes,
        // so this is a workaround :)
        if (_fixtureIsInitialized)
        {
            return;
        }

        await SetupContent();

        await SetupMedia();

        await SetupElements();

        _fixtureIsInitialized = true;
    }

    protected IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    protected IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    /// <summary>
    /// Gets the element containers at the library root, in creation order. Each holds ten library elements.
    /// </summary>
    protected EntityContainer[] ElementContainers { get; private set; } = [];

    /// <summary>
    /// Gets the element type of the library elements created at the library root.
    /// </summary>
    protected IContentType RootLibraryElementType { get; private set; } = null!;

    /// <summary>
    /// Gets the element type of the library elements created inside the element containers.
    /// </summary>
    protected IContentType ChildLibraryElementType { get; private set; } = null!;

    private async Task SetupElements()
    {
        RootLibraryElementType = await CreateLibraryElementType("rootLibraryElementType");
        ChildLibraryElementType = await CreateLibraryElementType("childLibraryElementType");

        var containers = new List<EntityContainer>();
        for (var i = 0; i < 3; i++)
        {
            await CreateLibraryElement(RootLibraryElementType, null, $"Root element {i}", $"library root title single{i}libroot");

            EntityContainer container = (await ElementContainerService.CreateAsync(Guid.NewGuid(), $"Folder {i}", null, Constants.Security.SuperUserKey)).Result
                                        ?? throw new InvalidOperationException("Could not create element container");
            containers.Add(container);

            for (var j = 0; j < 10; j++)
            {
                await CreateLibraryElement(
                    ChildLibraryElementType,
                    container.Key,
                    $"Element {j}",
                    $"library child title single{j}libchild triple{j / 3}libchild shared{i}lib");
            }
        }

        ElementContainers = containers.ToArray();
    }

    private async Task<IContentType> CreateLibraryElementType(string alias)
    {
        IContentType elementType = new ContentTypeBuilder()
            .WithAlias(alias)
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .WithAllowAsRoot(true)
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }

    private async Task CreateLibraryElement(IContentType elementType, Guid? containerKey, string name, string title)
    {
        Attempt<ElementCreateResult, ContentEditingOperationStatus> result = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = elementType.Key,
                ParentKey = containerKey,
                Variants = [new VariantModel { Name = name }],
                Properties = [new PropertyValueModel { Alias = "title", Value = title }],
            },
            Constants.Security.SuperUserKey);
        if (result.Success is false)
        {
            throw new InvalidOperationException($"Could not create library element: {result.Status}");
        }
    }

    private async Task SetupContent()
    {
        IContentType rootContentType = new ContentTypeBuilder()
            .WithAlias("rootContentType")
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(rootContentType, Constants.Security.SuperUserKey);

        IContentType childContentType = new ContentTypeBuilder()
            .WithAlias("childContentType")
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(childContentType, Constants.Security.SuperUserKey);

        rootContentType.AllowedContentTypes = [new ContentTypeSort(childContentType.Key, 0, childContentType.Alias)];
        await ContentTypeService.UpdateAsync(rootContentType, Constants.Security.SuperUserKey);

        for (var i = 0; i < 3; i++)
        {
            Content root = new ContentBuilder()
                .WithContentType(rootContentType)
                .WithName($"Root {i} shared shared{i}")
                .WithPropertyValues(
                    new
                    {
                        title = $"root title single{i}root"
                    })
                .Build();
            ContentService.Save(root);

            for (var j = 0; j < 10; j++)
            {
                // need to sleep for consistent datetime ordering results
                Thread.Sleep(20);
                Content child = new ContentBuilder()
                    .WithContentType(childContentType)
                    .WithName($"Child {j}")
                    .WithParent(root)
                    .WithPropertyValues(
                        new
                        {
                            title = $"child title single{j}child triple{j / 3}child oddeven{j % 2}child shared shared{i}"
                        })
                    .Build();
                ContentService.Save(child);
            }
        }
    }

    private async Task SetupMedia()
    {
        IMediaType rootMediaType = new MediaTypeBuilder()
            .WithAlias("rootMediaType")
            .AddPropertyGroup()
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Done()
            .Build();
        await MediaTypeService.CreateAsync(rootMediaType, Constants.Security.SuperUserKey);

        IMediaType childMediaType = new MediaTypeBuilder()
            .WithAlias("childMediaType")
            .AddPropertyGroup()
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Done()
            .Build();
        await MediaTypeService.CreateAsync(childMediaType, Constants.Security.SuperUserKey);

        rootMediaType.AllowedContentTypes = [new ContentTypeSort(childMediaType.Key, 0, childMediaType.Alias)];
        await MediaTypeService.UpdateAsync(rootMediaType, Constants.Security.SuperUserKey);

        for (var i = 0; i < 3; i++)
        {
            Media folder = new MediaBuilder()
                .WithMediaType(rootMediaType)
                .WithName($"Root {i} shared shared{i}")
                .WithPropertyValues(
                    new
                    {
                        title = $"root title single{i}root"
                    })
                .Build();
            MediaService.Save(folder);

            for (var j = 0; j < 10; j++)
            {
                // need to sleep for consistent datetime ordering results
                Thread.Sleep(20);
                Media child = new MediaBuilder()
                    .WithMediaType(childMediaType)
                    .WithName($"Child {j}")
                    .WithPropertyValues(
                        new
                        {
                            title = $"child title single{j}child triple{j / 3}child oddeven{j % 2}child shared shared{i}"
                        })
                    .WithParentId(folder.Id)
                    .Build();
                MediaService.Save(child);
            }
        }
    }
}
