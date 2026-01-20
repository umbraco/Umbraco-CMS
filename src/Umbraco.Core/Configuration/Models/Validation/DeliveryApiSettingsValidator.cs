// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration represented as <see cref="DeliveryApiSettings" />.
/// </summary>
public class DeliveryApiSettingsValidator : IValidateOptions<DeliveryApiSettings>
{
    private readonly ILogger<DeliveryApiSettingsValidator> _logger;

    public DeliveryApiSettingsValidator(ILogger<DeliveryApiSettingsValidator> logger)
        => _logger = logger;

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, DeliveryApiSettings options)
    {
        ValidateContentTypeAliasOverlap(options);

        return ValidateOptionsResult.Success;
    }

    private void ValidateContentTypeAliasOverlap(DeliveryApiSettings options)
    {
        if (options.AllowedContentTypeAliases.Count == 0 || options.DisallowedContentTypeAliases.Count == 0)
        {
            return;
        }

        var overlappingAliases = options.AllowedContentTypeAliases
            .Where(alias => options.DisallowedContentTypeAliases.InvariantContains(alias))
            .ToArray();

        if (overlappingAliases.Length > 0)
        {
            _logger.LogWarning(
                "Delivery API configuration contains content type aliases that appear in both AllowedContentTypeAliases and DisallowedContentTypeAliases: {Aliases}. " +
                "The allow list takes precedence, so these content types will be allowed. Consider removing them from the disallow list to avoid confusion.",
                string.Join(", ", overlappingAliases));
        }
    }
}
