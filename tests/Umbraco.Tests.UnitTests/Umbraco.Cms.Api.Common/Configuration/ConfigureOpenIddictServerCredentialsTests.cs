using System.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using OpenIddict.Server;
using OpenIddict.Validation;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.Configuration;

[TestFixture]
internal class ConfigureOpenIddictServerCredentialsTests
{
    private Dictionary<string, string> _keyValues = null!;
    private Mock<IKeyValueService> _keyValueService = null!;
    private Mock<ICoreScope> _scope = null!;
    private Mock<ICoreScopeProvider> _scopeProvider = null!;
    private Mock<IOptionsMonitorCache<OpenIddictServerOptions>> _serverOptionsCache = null!;
    private Mock<IOptionsMonitorCache<OpenIddictValidationOptions>> _validationOptionsCache = null!;
    private FakeTimeProvider _timeProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _keyValues = new Dictionary<string, string>();

        _keyValueService = new Mock<IKeyValueService>();
        _keyValueService
            .Setup(x => x.GetValue(It.IsAny<string>()))
            .Returns<string>(key => _keyValues.GetValueOrDefault(key));
        _keyValueService
            .Setup(x => x.SetValue(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((key, value) => _keyValues[key] = value);

        _scope = new Mock<ICoreScope>();
        _scopeProvider = new Mock<ICoreScopeProvider>();
        _scopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(_scope.Object);

        _serverOptionsCache = new Mock<IOptionsMonitorCache<OpenIddictServerOptions>>();
        _validationOptionsCache = new Mock<IOptionsMonitorCache<OpenIddictValidationOptions>>();
        _timeProvider = new FakeTimeProvider();
    }

    [Test]
    public void Ephemeral_Credentials_Are_Used_When_Persisted_Credentials_Cannot_Be_Loaded()
    {
        _keyValueService
            .Setup(x => x.GetValue(It.IsAny<string>()))
            .Throws(new InvalidOperationException("Database unavailable."));

        OpenIddictServerOptions options = Configure(RuntimeLevel.Run, new EphemeralDataProtectionProvider());

        Assert.Multiple(() =>
        {
            Assert.That(options.SigningCredentials, Has.Count.EqualTo(1));
            Assert.That(options.EncryptionCredentials, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Options_Are_Rebuilt_After_A_Delay_When_Persisted_Credentials_Cannot_Be_Loaded()
    {
        _keyValueService
            .Setup(x => x.GetValue(It.IsAny<string>()))
            .Throws(new InvalidOperationException("Database unavailable."));
        var validationOptionsReset = new TaskCompletionSource();
        _validationOptionsCache
            .Setup(x => x.TryRemove(Options.DefaultName))
            .Callback(() => validationOptionsReset.TrySetResult())
            .Returns(true);

        Configure(RuntimeLevel.Run, new EphemeralDataProtectionProvider());

        _timeProvider.Advance(ConfigureOpenIddictServerCredentials.PersistedCredentialsRetryDelay - TimeSpan.FromSeconds(1));
        _serverOptionsCache.Verify(x => x.TryRemove(It.IsAny<string>()), Times.Never);

        _timeProvider.Advance(TimeSpan.FromSeconds(1));
        await validationOptionsReset.Task.WaitAsync(TimeSpan.FromSeconds(10));
        _serverOptionsCache.Verify(x => x.TryRemove(Options.DefaultName), Times.Once);
    }

    [Test]
    public void Options_Are_Not_Rebuilt_When_Persisted_Credentials_Are_Loaded()
    {
        Configure(RuntimeLevel.Run, new EphemeralDataProtectionProvider());

        _timeProvider.Advance(ConfigureOpenIddictServerCredentials.PersistedCredentialsRetryDelay);

        _serverOptionsCache.Verify(x => x.TryRemove(It.IsAny<string>()), Times.Never);
        _validationOptionsCache.Verify(x => x.TryRemove(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Servers_Sharing_Database_And_Key_Ring_Use_The_Same_Credentials()
    {
        var sharedKeyRing = new EphemeralDataProtectionProvider();

        OpenIddictServerOptions server1 = Configure(RuntimeLevel.Run, sharedKeyRing);
        OpenIddictServerOptions server2 = Configure(RuntimeLevel.Run, sharedKeyRing);

        Assert.Multiple(() =>
        {
            Assert.That(GetSigningKeyModulus(server2), Is.EqualTo(GetSigningKeyModulus(server1)));
            Assert.That(GetEncryptionKey(server2), Is.EqualTo(GetEncryptionKey(server1)));
        });
    }

    [Test]
    public void Credentials_Are_Created_And_Persisted_Protected_When_None_Exist()
    {
        OpenIddictServerOptions options = Configure(RuntimeLevel.Run, new EphemeralDataProtectionProvider());

        Assert.Multiple(() =>
        {
            Assert.That(options.SigningCredentials, Has.Count.EqualTo(1));
            Assert.That(options.SigningCredentials[0].Key, Is.InstanceOf<RsaSecurityKey>());
            Assert.That(options.EncryptionCredentials, Has.Count.EqualTo(1));
            Assert.That(options.EncryptionCredentials[0].Key, Is.InstanceOf<SymmetricSecurityKey>());

            Assert.That(_keyValues, Contains.Key(ConfigureOpenIddictServerCredentials.KeyValueKey));
            Assert.That(_keyValues[ConfigureOpenIddictServerCredentials.KeyValueKey], Does.Not.Contain("SigningKey"));
        });

        _scope.Verify(x => x.WriteLock(Constants.Locks.KeyValues), Times.Once);
        _scope.Verify(x => x.Complete(), Times.Once);
    }

    [Test]
    public void Persisted_Credentials_Are_Read_Without_Taking_A_Lock()
    {
        var sharedKeyRing = new EphemeralDataProtectionProvider();
        Configure(RuntimeLevel.Run, sharedKeyRing);
        _scope.Invocations.Clear();
        _keyValueService.Invocations.Clear();

        Configure(RuntimeLevel.Run, sharedKeyRing);

        _scope.Verify(x => x.WriteLock(It.IsAny<int[]>()), Times.Never);
        _keyValueService.Verify(x => x.SetValue(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Credentials_Are_Replaced_When_Persisted_Credentials_Cannot_Be_Unprotected()
    {
        OpenIddictServerOptions server1 = Configure(RuntimeLevel.Run, new EphemeralDataProtectionProvider());
        var originalValue = _keyValues[ConfigureOpenIddictServerCredentials.KeyValueKey];

        OpenIddictServerOptions server2 = Configure(RuntimeLevel.Run, new EphemeralDataProtectionProvider());

        Assert.Multiple(() =>
        {
            Assert.That(GetSigningKeyModulus(server2), Is.Not.EqualTo(GetSigningKeyModulus(server1)));
            Assert.That(_keyValues[ConfigureOpenIddictServerCredentials.KeyValueKey], Is.Not.EqualTo(originalValue));
        });
    }

    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.Upgrading)]
    public void Credentials_Are_Persisted_When_The_Database_Is_Available(RuntimeLevel runtimeLevel)
    {
        Configure(runtimeLevel, new EphemeralDataProtectionProvider());

        Assert.That(_keyValues, Contains.Key(ConfigureOpenIddictServerCredentials.KeyValueKey));
    }

    [TestCase(RuntimeLevel.Unknown)]
    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.BootFailed)]
    public void Ephemeral_Credentials_Are_Used_When_The_Database_Is_Unavailable(RuntimeLevel runtimeLevel)
    {
        OpenIddictServerOptions options = Configure(runtimeLevel, new EphemeralDataProtectionProvider());

        Assert.Multiple(() =>
        {
            Assert.That(options.SigningCredentials, Has.Count.EqualTo(1));
            Assert.That(options.EncryptionCredentials, Has.Count.EqualTo(1));
        });
        _keyValueService.VerifyNoOtherCalls();
        _scopeProvider.VerifyNoOtherCalls();
    }

    private OpenIddictServerOptions Configure(RuntimeLevel runtimeLevel, IDataProtectionProvider dataProtectionProvider)
    {
        var runtimeState = new Mock<IRuntimeState>();
        runtimeState.SetupGet(x => x.Level).Returns(runtimeLevel);

        var sut = new ConfigureOpenIddictServerCredentials(
            runtimeState.Object,
            _scopeProvider.Object,
            _keyValueService.Object,
            dataProtectionProvider,
            NullLogger<ConfigureOpenIddictServerCredentials>.Instance,
            _serverOptionsCache.Object,
            _validationOptionsCache.Object,
            _timeProvider);

        var options = new OpenIddictServerOptions();
        sut.Configure(options);
        return options;
    }

    private static byte[] GetSigningKeyModulus(OpenIddictServerOptions options)
        => ((RsaSecurityKey)options.SigningCredentials.Single().Key).Rsa.ExportParameters(false).Modulus!;

    private static byte[] GetEncryptionKey(OpenIddictServerOptions options)
        => ((SymmetricSecurityKey)options.EncryptionCredentials.Single().Key).Key;
}
