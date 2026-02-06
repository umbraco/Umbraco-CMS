using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

/// <summary>
/// Ensures that the Media Document Type called "Folder" gets a collection added to it.
/// </summary>
public class EnsureDefaultMediaFolderHasDefaultCollection : AsyncMigrationBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IDataTypeService _dataTypeService;

    public EnsureDefaultMediaFolderHasDefaultCollection(
        IMigrationContext context,
        IMediaTypeService mediaTypeService,
        IDataTypeService dataTypeService)
        : base(context)
    {
        _mediaTypeService = mediaTypeService;
        _dataTypeService = dataTypeService;
    }

    protected override async Task MigrateAsync()
    {
        IMediaType? folderMediaType = _mediaTypeService
            .Get(Constants.MediaTypes.Guids.FolderGuid);

        if (folderMediaType is null || folderMediaType.ListView is not null)
        {
            return;
        }

        IDataType? dataType = await _dataTypeService.GetAsync(Guid.Parse("3a0156c4-3b8c-4803-bdc1-6871faa83fff")); // Media Collection default key.

        if (dataType is null)
        {
            return;
        }

        folderMediaType.ListView = dataType.Key;
        await _mediaTypeService.UpdateAsync(folderMediaType, Constants.Security.SuperUserKey);
    }
}
