using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;

namespace Umbraco.Cms.Web.BackOffice.Trees;

[Tree(Constants.Applications.Settings, "files", TreeTitle = "Files", TreeUse = TreeUse.Dialog)]
[CoreTree]
public class FilesTreeController : FileSystemTreeController
{
    private static readonly string[] ExtensionsStatic = { "*" };

    public FilesTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        IPhysicalFileSystem fileSystem,
        IEventAggregator eventAggregator)
        : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, eventAggregator) =>
        FileSystem = fileSystem;

    protected override IFileSystem FileSystem { get; }

    protected override string[] Extensions => ExtensionsStatic;

    protected override string FileIcon => Constants.Icons.MediaFile;
}
