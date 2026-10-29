using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.SchemaLockdown.Configurators;

/// <summary>
/// Locks templates and partial views when the site runs in <see cref="RuntimeMode.Production"/>.
/// </summary>
/// <remarks>
/// In production runtime mode template and partial view files are not written to disk, as they arrive with the
/// deployment. A change requested through the Management API could not be persisted, so it is denied rather than
/// appearing to succeed.
/// <para>
/// Deleting a template stays permitted. It removes only the database record, leaving the file to the deployment,
/// which is how a template removed from the code is cleaned up on the environments it was deployed to. A partial
/// view is only its file, so there is nothing to clean up and deleting one is denied.
/// </para>
/// <para>
/// For templates this is the only thing denying a change. A site that removes this configurator lets template
/// changes through in production runtime mode, saving them to the database without writing their files.
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
