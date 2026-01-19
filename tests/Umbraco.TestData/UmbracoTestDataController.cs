using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;
using Umbraco.TestData.Configuration;

namespace Umbraco.TestData;

/// <summary>
/// Controller for creating test data in Umbraco CMS.
/// Provides functionality to generate hierarchical content and media trees for testing purposes.
/// </summary>
/// <remarks>
/// This controller requires the TestData settings to be enabled in configuration.
/// It is intended for testing purposes only and should not be used in production environments.
/// </remarks>
public class UmbracoTestDataController : SurfaceController
{
    private const string TestDataContentTypeAlias = "umbTestDataContent";
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly TestDataSettings _testDataSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoTestDataController"/> class.
    /// </summary>
    /// <param name="umbracoContextAccessor">The Umbraco context accessor.</param>
    /// <param name="databaseFactory">The database factory.</param>
    /// <param name="services">The service context.</param>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="profilingLogger">The profiling logger.</param>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    /// <param name="scopeProvider">The core scope provider.</param>
    /// <param name="propertyEditors">The property editor collection.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="testDataSettings">The test data settings.</param>
    public UmbracoTestDataController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        ICoreScopeProvider scopeProvider,
        PropertyEditorCollection propertyEditors,
        IShortStringHelper shortStringHelper,
        IOptions<TestDataSettings> testDataSettings)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _scopeProvider = scopeProvider;
        _propertyEditors = propertyEditors;
        _shortStringHelper = shortStringHelper;
        _testDataSettings = testDataSettings.Value;
    }

    /// <summary>
    /// Creates a content and associated media tree (hierarchy) for testing purposes.
    /// </summary>
    /// <param name="count">The total number of items to create in the tree.</param>
    /// <param name="depth">The maximum depth of the tree hierarchy.</param>
    /// <param name="locale">The locale to use for generating fake data (default: "en").</param>
    /// <returns>A text response indicating completion.</returns>
    /// <remarks>
    /// Each content item created is associated to a media item via a media picker,
    /// therefore a relation is created between the two. The content is populated
    /// with fake product data including reviews and descriptions.
    /// </remarks>
    public async Task<IActionResult> CreateTree(int count, int depth, string locale = "en")
    {
        if (_testDataSettings.Enabled == false)
        {
            return NotFound();
        }

        if (!Validate(count, depth, out var message, out var perLevel))
        {
            throw new InvalidOperationException(message);
        }

        var faker = new Faker(locale);
        var company = faker.Company.CompanyName();

        using (var scope = _scopeProvider.CreateCoreScope())
        {
            var imageIds = CreateMediaTree(company, faker, count, depth).ToList();
            var (contentIds, root) = await CreateContentTreeAsync(company, faker, count, depth, imageIds);

            // Force enumeration of the lazy content hierarchy (CreateHierarchy uses yield return)
            // so that all content is created before we publish the root branch.
            _ = contentIds.ToList();

            Services.ContentService.PublishBranch(root, PublishBranchFilter.IncludeUnpublished, ["*"]);

            scope.Complete();
        }


        return Content("Done");
    }

    /// <summary>
    /// Validates the count and depth parameters for tree creation.
    /// </summary>
    /// <param name="count">The total number of items to create.</param>
    /// <param name="depth">The maximum depth of the tree.</param>
    /// <param name="message">The error message if validation fails.</param>
    /// <param name="perLevel">The calculated number of items per level.</param>
    /// <returns><c>true</c> if validation passes; otherwise, <c>false</c>.</returns>
    private static bool Validate(int count, int depth, out string message, out int perLevel)
    {
        perLevel = 0;
        message = null;

        if (count <= 0)
        {
            message = "Count must be more than 0";
            return false;
        }

        perLevel = count / depth;
        if (perLevel < 1)
        {
            message = "Count not high enough for specified for number of levels required";
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Utility to create a tree hierarchy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <param name="count"></param>
    /// <param name="depth"></param>
    /// <param name="create">
    ///     A callback that returns a tuple of Content and another callback to produce a Container.
    ///     For media, a container will be another folder, for content the container will be the Content itself.
    /// </param>
    /// <returns></returns>
    private IEnumerable<Udi> CreateHierarchy<T>(T parent, int count, int depth, Func<T, (T content, Func<T> container)> create)
        where T : class, IContentBase
    {
        yield return parent.GetUdi();

        // This will not calculate a balanced tree but it will ensure that there will be enough nodes deep enough to not fill up the tree.
        var totalDescendants = count - 1;
        var perLevel = Math.Ceiling(totalDescendants / (double)depth);
        var perBranch = Math.Ceiling(perLevel / depth);

        var tracked = new Stack<(T parent, int childCount)>();

        var currChildCount = 0;

        for (var i = 0; i < count; i++)
        {
            var (content, container) = create(parent);
            var contentItem = content;

            yield return contentItem.GetUdi();

            currChildCount++;

            if (currChildCount == perBranch)
            {
                // move back up...

                var prev = tracked.Pop();

                // restore child count
                currChildCount = prev.childCount;
                // restore the parent
                parent = prev.parent;
            }
            else if (contentItem.Level < depth)
            {
                // track the current parent and it's current child count
                tracked.Push((parent, currChildCount));

                // not at max depth, create below
                parent = container();

                currChildCount = 0;
            }
        }
    }

    /// <summary>
    ///     Creates the media tree hiearachy.
    /// </summary>
    /// <param name="company">The company name used as the root content name.</param>
    /// <param name="faker">The faker instance for generating test data.</param>
    /// <param name="count">The total number of content items to create.</param>
    /// <param name="depth">The maximum depth of the content hierarchy.</param>
    private IEnumerable<Udi> CreateMediaTree(string company, Faker faker, int count, int depth)
    {
        var parent =
            Services.MediaService.CreateMediaWithIdentity(company, -1, Constants.Conventions.MediaTypes.Folder);

        return CreateHierarchy(parent, count, depth, currParent =>
        {
            var imageUrl = faker.Image.PicsumUrl();

            // we are appending a &ext=.jpg to the end of this for a reason. The result of this URL will be something like:
            // https://picsum.photos/640/480/?image=106
            // and due to the way that we detect images there must be an extension so we'll change it to
            // https://picsum.photos/640/480/?image=106&ext=.jpg
            // which will trick our app into parsing this and thinking it's an image ... which it is so that's good.
            // if we don't do this we don't get thumbnails in the back office.
            imageUrl += "&ext=.jpg";

            var media = Services.MediaService.CreateMedia(faker.Commerce.ProductName(), currParent, Constants.Conventions.MediaTypes.Image);
            media.SetValue(Constants.Conventions.Media.File, imageUrl);
            Services.MediaService.Save(media);
            return (media, () =>
            {
                // create a folder to contain child media
                var container = Services.MediaService.CreateMediaWithIdentity(faker.Commerce.Department(), currParent, Constants.Conventions.MediaTypes.Folder);
                return container;
            }
            );
        });
    }

    /// <summary>
    ///     Creates the content tree hierarchy
    /// </summary>
    /// <param name="company"></param>
    /// <param name="faker"></param>
    /// <param name="count"></param>
    /// <param name="depth"></param>
    /// <param name="imageIds"></param>
    /// <returns>A tuple containing the UDIs of created content and the root content item.</returns>
    private async Task<(IEnumerable<Udi> udis, IContent root)> CreateContentTreeAsync(string company, Faker faker, int count, int depth, List<Udi> imageIds)
    {
        var random = new Random(company.GetHashCode());

        var docType = await GetOrCreateContentTypeAsync();

        var parent = Services.ContentService.Create(company, -1, docType.Alias);

        // give it some reasonable data (100 reviews)
        parent.SetValue("review", string.Join(" ", Enumerable.Range(0, 100).Select(x => faker.Rant.Review())));
        parent.SetValue("desc", company);
        parent.SetValue("media", imageIds[random.Next(0, imageIds.Count - 1)]);
        Services.ContentService.Save(parent);

        var udis = CreateHierarchy(parent, count, depth, currParent =>
        {
            var content = Services.ContentService.Create(faker.Commerce.ProductName(), currParent, docType.Alias);

            // give it some reasonable data (100 reviews)
            content.SetValue("review", string.Join(" ", Enumerable.Range(0, 100).Select(x => faker.Rant.Review())));
            content.SetValue("desc", string.Join(", ", Enumerable.Range(0, 5).Select(x => faker.Commerce.ProductAdjective())));
            content.SetValue("media", imageIds[random.Next(0, imageIds.Count - 1)]);

            Services.ContentService.Save(content);
            return (content, () => content);
        });

        return (udis, parent);
    }

    /// <summary>
    /// Gets the existing test data content type or creates it if it doesn't exist.
    /// </summary>
    /// <returns>The test data content type.</returns>
    /// <remarks>
    /// The content type includes properties for review (rich text), description (textbox),
    /// and media (media picker). It is configured to allow itself as a child content type
    /// for creating nested hierarchies.
    /// </remarks>
    private async Task<IContentType> GetOrCreateContentTypeAsync()
    {
        var docType = Services.ContentTypeService.Get(TestDataContentTypeAlias);
        if (docType != null)
        {
            return docType;
        }

        docType = new ContentType(_shortStringHelper, -1)
        {
            Alias = TestDataContentTypeAlias,
            Name = "Umbraco Test Data Content",
            Icon = "icon-science color-green"
        };
        docType.AddPropertyGroup("content", "Content");
        docType.AddPropertyType(
            new PropertyType(_shortStringHelper, Constants.PropertyEditors.Aliases.RichText, ValueStorageType.Ntext, "review")
            {
                Name = "Review",
            },
            "content");
        docType.AddPropertyType(
            new PropertyType(_shortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "desc") { Name = "Description" },
            "content");
        docType.AddPropertyType(
            new PropertyType(_shortStringHelper, Constants.PropertyEditors.Aliases.MediaPicker3, ValueStorageType.Integer, "media") { Name = "Media" },
            "content");


        await Services.ContentTypeService.CreateAsync(docType, Constants.Security.SuperUserKey);
        var createResult = await Services.ContentTypeService.CreateAsync(docType, Constants.Security.SuperUserKey);
        if (createResult.Success is false)
        {
            throw new InvalidOperationException($"Failed to create content type with alias {docType.Alias}.");
        }

        docType.AllowedContentTypes = [new ContentTypeSort(docType.Key, 0, docType.Alias)];
        var updateResult = await Services.ContentTypeService.UpdateAsync(docType, Constants.Security.SuperUserKey);
        if (updateResult.Success is false)
        {
            throw new InvalidOperationException($"Failed to update content type with alias {docType.Alias}.");
        }

        return docType;
    }
}
