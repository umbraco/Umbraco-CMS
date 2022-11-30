using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

[Obsolete("This is not used anymore and will be removed in Umbraco 13")]
public class DataTypeMigration : MigrationBase
{
    private static readonly ISet<string> _legacyAliases = new HashSet<string>
    {
        Constants.PropertyEditors.Legacy.Aliases.Date,
        Constants.PropertyEditors.Legacy.Aliases.Textbox,
        Constants.PropertyEditors.Legacy.Aliases.ContentPicker2,
        Constants.PropertyEditors.Legacy.Aliases.MediaPicker2,
        Constants.PropertyEditors.Legacy.Aliases.MemberPicker2,
        Constants.PropertyEditors.Legacy.Aliases.RelatedLinks2,
        Constants.PropertyEditors.Legacy.Aliases.TextboxMultiple,
        Constants.PropertyEditors.Legacy.Aliases.MultiNodeTreePicker2,
    };

    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;
    private readonly ILogger<DataTypeMigration> _logger;
    private readonly PreValueMigratorCollection _preValueMigrators;
    private readonly PropertyEditorCollection _propertyEditors;

    public DataTypeMigration(
        IMigrationContext context,
        PreValueMigratorCollection preValueMigrators,
        PropertyEditorCollection propertyEditors,
        ILogger<DataTypeMigration> logger,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(context)
    {
        _preValueMigrators = preValueMigrators;
        _propertyEditors = propertyEditors;
        _logger = logger;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
    }

    protected override void Migrate()
    {

    }
}
