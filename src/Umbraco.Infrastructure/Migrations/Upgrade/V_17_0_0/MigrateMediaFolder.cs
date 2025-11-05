using Microsoft.VisualBasic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

public class MigrateMediaFolder : AsyncMigrationBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IDataTypeService _dataTypeService;

    public MigrateMediaFolder(
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
            .Get(Guid.Parse("f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d")); // Folder media type default key.

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
