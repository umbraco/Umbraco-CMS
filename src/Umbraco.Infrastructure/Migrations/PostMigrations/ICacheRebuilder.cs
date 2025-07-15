namespace Umbraco.Cms.Infrastructure.Migrations.PostMigrations;

/// <summary>
///     Rebuilds the published snapshot.
/// </summary>
/// <remarks>
///     <para>
///         This interface exists because the entire published snapshot lives in Umbraco.Web
///         but we may want to trigger rebuilds from Umbraco.Core. These two assemblies should
///         be refactored, really.
///     </para>
/// </remarks>
[Obsolete("This is no longer used. Scheduled for removal in Umbraco 17.")]
public interface ICacheRebuilder
{
    /// <summary>
    ///     Rebuilds.
    /// </summary>
    void Rebuild();
}
