using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
/// Fails start-up when schema lockdown configuration contains entity types that could not be bound.
/// </summary>
internal class SchemaLockdownSettingsValidator : IValidateOptions<SchemaLockdownSettings>
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaLockdownSettingsValidator"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public SchemaLockdownSettingsValidator(IConfiguration configuration) => _configuration = configuration;

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, SchemaLockdownSettings options)
    {
        try
        {
            SchemaLockdownSettings.ValidateBinding(
                _configuration.GetSection(Constants.Configuration.ConfigSchemaLockdown));
        }
        catch (InvalidOperationException exception)
        {
            return ValidateOptionsResult.Fail(exception.Message);
        }

        return ValidateOptionsResult.Success;
    }
}
