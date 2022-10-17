using System.Reflection;
using System.Text;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_3_0;

public class AddBlockGridPartialViews : MigrationBase
{
    private const string FolderPath = "/Views/Partials/blockgrid";
    private const string AssemblyPath = "Umbraco.Cms.Core.EmbeddedResources.BlockGrid";
    private static readonly string[] _filesToAdd =
    {
        "areas.cshtml",
        "default.cshtml",
        "items.cshtml",
    };

    private readonly IFileService _fileService;

    public AddBlockGridPartialViews(IMigrationContext context, IFileService fileService) : base(context)
    {
        _fileService = fileService;
    }

    protected override void Migrate()
    {
        // Get the files from the embedded resources, just using typeof of anything from the core assembly.
        Assembly assembly = typeof(Constants).Assembly;

        foreach (var fileName in _filesToAdd)
        {
            Stream? content = assembly.GetManifestResourceStream($"{AssemblyPath}.{fileName}");
            if (content is not null)
            {
                var viewPath = $"{FolderPath}/{fileName}";

                // We have to ensure that this is idempotent, so only save the view if it does not already exist
                // We don't want to overwrite any changes made.
                IPartialView? existingView = _fileService.GetPartialView(viewPath);
                if (existingView is null)
                {
                    var view = new PartialView(PartialViewType.PartialView, viewPath)
                    {
                        Content = GetTextFromStream(content)
                    };

                    _fileService.SavePartialView(view);
                }
            }
        }
    }

    private string GetTextFromStream(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var streamReader = new StreamReader(stream, Encoding.UTF8);
        return streamReader.ReadToEnd();
    }
}
