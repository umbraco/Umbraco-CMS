using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using OpenIddict.Validation;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Common.Configuration;

/// <summary>
///     Configures the signing and encryption credentials of the OpenIddict server.
/// </summary>
/// <remarks>
///     Tokens issued in the ASP.NET Core Data Protection format do not use these credentials, but identity tokens
///     are always issued as JWTs signed with the signing credentials, and the signing keys are published to clients.
///     To ensure every server in a load balanced setup issues and accepts the same tokens, the credentials are
///     persisted in the database, protected with ASP.NET Core Data Protection. Until the database is available
///     (i.e. during install), ephemeral credentials are used.
/// </remarks>
internal sealed class ConfigureOpenIddictServerCredentials : IConfigureOptions<OpenIddictServerOptions>
{
    /// <summary>
    ///     The key-value store key holding the protected server credentials.
    /// </summary>
    internal const string KeyValueKey = "Umbraco.Core.OpenIddict.ServerCredentials";

    /// <summary>
    ///     The Data Protection purpose used to protect the persisted server credentials.
    /// </summary>
    internal const string DataProtectionPurpose = "Umbraco.OpenIddict.ServerCredentials";

    /// <summary>
    ///     The delay before retrying to load the persisted server credentials after a failure.
    /// </summary>
    internal static readonly TimeSpan PersistedCredentialsRetryDelay = TimeSpan.FromMinutes(1);

    private const int SigningKeySizeInBits = 2048;
    private const int EncryptionKeySizeInBytes = 32;

    private readonly IRuntimeState _runtimeState;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IKeyValueService _keyValueService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger<ConfigureOpenIddictServerCredentials> _logger;
    private readonly IOptionsMonitorCache<OpenIddictServerOptions> _serverOptionsCache;
    private readonly IOptionsMonitorCache<OpenIddictValidationOptions> _validationOptionsCache;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureOpenIddictServerCredentials"/> class.
    /// </summary>
    /// <param name="runtimeState">The runtime state, used to determine whether the database is available.</param>
    /// <param name="scopeProvider">The core scope provider.</param>
    /// <param name="keyValueService">The key-value service used to persist the credentials.</param>
    /// <param name="dataProtectionProvider">The Data Protection provider used to protect the persisted credentials.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="serverOptionsCache">The cache of the OpenIddict server options, cleared to retry after a failure.</param>
    /// <param name="validationOptionsCache">The cache of the OpenIddict validation options, which copy the server credentials.</param>
    /// <param name="timeProvider">The time provider used to delay the retry.</param>
    public ConfigureOpenIddictServerCredentials(
        IRuntimeState runtimeState,
        ICoreScopeProvider scopeProvider,
        IKeyValueService keyValueService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<ConfigureOpenIddictServerCredentials> logger,
        IOptionsMonitorCache<OpenIddictServerOptions> serverOptionsCache,
        IOptionsMonitorCache<OpenIddictValidationOptions> validationOptionsCache,
        TimeProvider timeProvider)
    {
        _runtimeState = runtimeState;
        _scopeProvider = scopeProvider;
        _keyValueService = keyValueService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
        _serverOptionsCache = serverOptionsCache;
        _validationOptionsCache = validationOptionsCache;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public void Configure(OpenIddictServerOptions options)
    {
        ServerCredentials credentials = _runtimeState.Level is RuntimeLevel.Run or RuntimeLevel.Upgrade or RuntimeLevel.Upgrading
            ? GetPersistedCredentialsOrFallback()
            : ServerCredentials.Create();

        var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(credentials.SigningKey, out _);

        options
            .SigningCredentials
            .Add(new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256));
        options
            .EncryptionCredentials
            .Add(new EncryptingCredentials(
                new SymmetricSecurityKey(credentials.EncryptionKey),
                SecurityAlgorithms.Aes256KW,
                SecurityAlgorithms.Aes256CbcHmacSha512));
    }

    private ServerCredentials GetPersistedCredentialsOrFallback()
    {
        try
        {
            return GetOrCreatePersistedCredentials();
        }
        catch (Exception ex)
        {
            // The options cache keeps a failed build for the lifetime of the application, so a transient database
            // error must not escape. Use ephemeral credentials for now, and rebuild the options later to retry.
            _logger.LogError(
                ex,
                "The persisted OpenIddict server credentials could not be loaded, so ephemeral credentials are used until the next attempt in {RetryDelay}.",
                PersistedCredentialsRetryDelay);
            _ = ResetOptionsAfterDelayAsync();
            return ServerCredentials.Create();
        }
    }

    private async Task ResetOptionsAfterDelayAsync()
    {
        await Task.Delay(PersistedCredentialsRetryDelay, _timeProvider);
        _serverOptionsCache.TryRemove(Options.DefaultName);
        _validationOptionsCache.TryRemove(Options.DefaultName);
    }

    private ServerCredentials GetOrCreatePersistedCredentials()
    {
        IDataProtector dataProtector = _dataProtectionProvider.CreateProtector(DataProtectionPurpose);

        // The credentials only need creating once, so read without a lock to avoid queueing behind other writers.
        if (TryReadCredentials(dataProtector, logFailure: true, out ServerCredentials? credentials))
        {
            return credentials;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.KeyValues);

        // Another server may have created the credentials while we were waiting for the lock.
        if (TryReadCredentials(dataProtector, logFailure: false, out credentials) is false)
        {
            credentials = ServerCredentials.Create();
            _keyValueService.SetValue(KeyValueKey, dataProtector.Protect(JsonSerializer.Serialize(credentials)));
            _logger.LogInformation("Created and persisted new OpenIddict server credentials.");
        }

        scope.Complete();
        return credentials;
    }

    private bool TryReadCredentials(IDataProtector dataProtector, bool logFailure, [NotNullWhen(true)] out ServerCredentials? credentials)
    {
        credentials = null;
        var protectedValue = _keyValueService.GetValue(KeyValueKey);
        if (string.IsNullOrEmpty(protectedValue))
        {
            return false;
        }

        try
        {
            credentials = JsonSerializer.Deserialize<ServerCredentials>(dataProtector.Unprotect(protectedValue));
        }
        catch (Exception ex) when (ex is CryptographicException or JsonException)
        {
            if (logFailure)
            {
                _logger.LogWarning(
                    ex,
                    "The persisted OpenIddict server credentials could not be decrypted, so new ones will be created. This happens when the Data Protection key ring or application name has changed since they were stored. In a load balanced setup, make sure all servers share the same key ring and application name, otherwise they will not use the same credentials.");
            }

            return false;
        }

        return credentials is not null;
    }

    private sealed record ServerCredentials(byte[] SigningKey, byte[] EncryptionKey)
    {
        public static ServerCredentials Create()
        {
            using var rsa = RSA.Create(SigningKeySizeInBits);
            return new ServerCredentials(rsa.ExportPkcs8PrivateKey(), RandomNumberGenerator.GetBytes(EncryptionKeySizeInBytes));
        }
    }
}
