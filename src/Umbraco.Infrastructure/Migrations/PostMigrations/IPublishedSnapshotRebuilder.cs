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
public interface IPublishedSnapshotRebuilder
{
    /// <summary>
    ///     Rebuilds.
    /// </summary>
    void Rebuild();
}
