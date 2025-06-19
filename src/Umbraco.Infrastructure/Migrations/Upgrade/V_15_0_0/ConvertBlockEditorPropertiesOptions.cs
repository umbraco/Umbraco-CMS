namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

[Obsolete("Will be removed in V18")]
public class ConvertBlockEditorPropertiesOptions
{
    /// <summary>
    /// Setting this property to true will cause the migration of Block List editors to be skipped.
    /// </summary>
    /// <remarks>
    /// If you choose to skip the migration, you're responsible for performing the content migration for Block Lists after the V15 upgrade has completed.
    /// </remarks>
    public bool SkipBlockListEditors { get; set; } = false;

    /// <summary>
    /// Setting this property to true will cause the migration of Block Grid editors to be skipped.
    /// </summary>
    /// <remarks>
    /// If you choose to skip the migration, you're responsible for performing the content migration for Block Grids after the V15 upgrade has completed.
    /// </remarks>
    public bool SkipBlockGridEditors { get; set; } = false;

    /// <summary>
    /// Setting this property to true will cause the migration of Rich Text editors to be skipped.
    /// </summary>
    /// <remarks>
    /// If you choose to skip the migration, you're responsible for performing the content migration for Rich Texts after the V15 upgrade has completed.
    /// </remarks>
    public bool SkipRichTextEditors { get; set; } = false;

    /// <summary>
    /// Setting this property to true will cause all block editor migrations to run as parallel operations.
    /// </summary>
    /// <remarks>
    /// While this greatly improves the speed of the migration, some content setups may experience issues and failing migrations as a result.
    /// </remarks>
    public bool ParallelizeMigration { get; set; } = false;
}
