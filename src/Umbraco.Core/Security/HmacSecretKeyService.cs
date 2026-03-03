using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Security;

/// <inheritdoc />
internal sealed class HmacSecretKeyService : IHmacSecretKeyService
{
    private const int KeySizeInBytes = 64;

    private readonly IConfigManipulator _configManipulator;
    private readonly ILogger<HmacSecretKeyService> _logger;
    private ImagingSettings _imagingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HmacSecretKeyService" /> class.
    /// </summary>
    /// <param name="optionsMonitor">The options monitor for imaging settings.</param>
    /// <param name="configManipulator">The configuration manipulator.</param>
    /// <param name="logger">The logger.</param>
    public HmacSecretKeyService(
        IOptionsMonitor<ImagingSettings> optionsMonitor,
        IConfigManipulator configManipulator,
        ILogger<HmacSecretKeyService> logger)
    {
        _imagingSettings = optionsMonitor.CurrentValue;
        optionsMonitor.OnChange(settings => _imagingSettings = settings);
        _configManipulator = configManipulator;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool HasHmacSecretKey()
        => _imagingSettings.HMACSecretKey.Length > 0;

    /// <inheritdoc />
    public async Task<Attempt<HmacSecretKeyOperationStatus>> CreateHmacSecretKeyAsync()
    {
        if (HasHmacSecretKey())
        {
            _logger.LogInformation("Imaging HMAC secret key already exists, skipping generation.");
            return Attempt<HmacSecretKeyOperationStatus>.Fail(HmacSecretKeyOperationStatus.KeyExists);
        }

        byte[] key = RandomNumberGenerator.GetBytes(KeySizeInBytes);
        var base64Key = Convert.ToBase64String(key);

        try
        {
            await _configManipulator.SetImagingHmacSecretKeyAsync(base64Key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't update config files with an imaging HMAC secret key");
            return Attempt<HmacSecretKeyOperationStatus>.Fail(HmacSecretKeyOperationStatus.Error, ex);
        }

        _logger.LogInformation("Imaging HMAC secret key has been generated and saved to configuration.");
        return Attempt<HmacSecretKeyOperationStatus>.Succeed(HmacSecretKeyOperationStatus.Success);
    }
}
