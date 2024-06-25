using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class MigrateRichTextConfiguration : MigrationBase
{
    private readonly IDataTypeService _dataTypeService;

    public MigrateRichTextConfiguration(IMigrationContext context, IDataTypeService dataTypeService) : base(context) => _dataTypeService = dataTypeService;

    protected override void Migrate()
    {
        IEnumerable<IDataType> richTextEditors = _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.RichText).GetAwaiter().GetResult();
        foreach (IDataType richTextEditor in richTextEditors)
        {
            if (!richTextEditor.ConfigurationData.TryGetValue("toolbar", out var configurationValue))
            {
                continue;
            }

            if (configurationValue is not List<string> toolbar)
            {
                continue;
            }

            if (toolbar.Contains("ace"))
            {
                toolbar.Remove("ace");
                toolbar.Add("sourcecode");
            }

            richTextEditor.ConfigurationData.Remove("toolbar");
            richTextEditor.ConfigurationData.Add("toolbar", toolbar);
            _dataTypeService.UpdateAsync(richTextEditor, Constants.Security.SuperUserKey);
        }
    }
}
