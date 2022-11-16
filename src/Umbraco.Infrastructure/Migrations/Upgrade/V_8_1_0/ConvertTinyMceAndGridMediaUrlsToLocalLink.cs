using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_1_0;

[Obsolete("This is not used anymore and will be removed in Umbraco 13")]
public class ConvertTinyMceAndGridMediaUrlsToLocalLink : MigrationBase
{
    private readonly IMediaService _mediaService;

    public ConvertTinyMceAndGridMediaUrlsToLocalLink(IMigrationContext context, IMediaService mediaService)
        : base(context) => _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));

    protected override void Migrate()
    {

    }
}
