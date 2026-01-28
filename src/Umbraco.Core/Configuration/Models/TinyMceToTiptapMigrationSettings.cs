namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for TinyMCE to Tiptap editor migration settings.
/// </summary>
public class TinyMceToTiptapMigrationSettings
{
    /// <summary>
    ///     Gets or sets a value indicating whether the migration from TinyMCE to Tiptap is disabled.
    /// </summary>
    public bool DisableMigration { get; set; }
}
