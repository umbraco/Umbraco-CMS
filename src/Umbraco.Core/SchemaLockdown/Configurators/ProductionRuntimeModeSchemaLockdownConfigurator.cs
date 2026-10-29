using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.SchemaLockdown.Configurators;

/// <summary>
/// Locks templates and partial views when the site runs in <see cref="RuntimeMode.Production"/>.
/// </summary>
/// <remarks>
/// In production runtime mode template and partial view files come with the deployment and aren't written to disk,
/// so a change would either be lost or, for a template's database record, drift from its file.
/// <para>
/// Deleting a template is still permitted: it removes only the database record, which is how a template removed from
/// the code is cleaned up. A partial view is only its file, so deleting one is denied.
/// </para>
/// <para>
/// Removing this configurator lets template changes through in production runtime mode, saved to the database without
/// their files.
/// </para>
/// </remarks>
public sealed class ProductionRuntimeModeSchemaLockdownConfigurator : ISchemaLockdownConfigurator
{
    private readonly IOptions<RuntimeSettings> _runtimeSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductionRuntimeModeSchemaLockdownConfigurator"/> class.
    /// </summary>
    /// <param name="runtimeSettings">The runtime settings.</param>
    public ProductionRuntimeModeSchemaLockdownConfigurator(IOptions<RuntimeSettings> runtimeSettings)
        => _runtimeSettings = runtimeSettings;

    /// <inheritdoc />
    public void Configure(ISchemaRestrictionsBuilder builder)
    {
        if (_runtimeSettings.Value.Mode != RuntimeMode.Production)
        {
            return;
        }

        builder.Block(Constants.UdiEntityType.Template, SchemaOperation.Create, SchemaOperation.Update);
        builder.BlockMutations(Constants.UdiEntityType.PartialView);
    }
}
