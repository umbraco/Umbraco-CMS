// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Sync;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Sync;

[TestFixture]
public class BatchedDatabaseServerMessengerTests
{
    private Mock<IMainDom> _mainDomMock = null!;
    private Mock<ICacheInstructionService> _cacheInstructionServiceMock = null!;
    private Mock<ILogger<BatchedDatabaseServerMessenger>> _loggerMock = null!;
    private Mock<ICacheRefresher> _cacheRefresherMock = null!;

    [SetUp]
    public void SetUp()
    {
        _mainDomMock = new Mock<IMainDom>();
        _cacheInstructionServiceMock = new Mock<ICacheInstructionService>();
        _loggerMock = new Mock<ILogger<BatchedDatabaseServerMessenger>>();
        _cacheRefresherMock = new Mock<ICacheRefresher>();
        _cacheRefresherMock.SetupGet(x => x.RefresherUniqueId).Returns(Guid.NewGuid());
    }

    [Test]
    public void QueueRefresh_WhenMainDomIsRegistered_DeliversRemoteInstructions()
    {
        BatchedDatabaseServerMessenger sut = CreateMessenger(mainDomRegistered: true);

        sut.QueueRefresh(_cacheRefresherMock.Object, 1234);

        _cacheInstructionServiceMock.Verify(
            x => x.DeliverInstructionsInBatches(It.IsAny<IEnumerable<RefreshInstruction>>(), It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public void QueueRefresh_WhenMainDomRegistrationFails_StillDeliversRemoteInstructions()
    {
        BatchedDatabaseServerMessenger sut = CreateMessenger(mainDomRegistered: false);

        sut.QueueRefresh(_cacheRefresherMock.Object, 1234);

        _cacheInstructionServiceMock.Verify(
            x => x.DeliverInstructionsInBatches(It.IsAny<IEnumerable<RefreshInstruction>>(), It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public void QueueRefresh_WhenMainDomRegistrationFails_LogsError()
    {
        BatchedDatabaseServerMessenger sut = CreateMessenger(mainDomRegistered: false);

        sut.QueueRefresh(_cacheRefresherMock.Object, 1234);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private BatchedDatabaseServerMessenger CreateMessenger(bool mainDomRegistered)
    {
        _mainDomMock
            .Setup(x => x.Register(It.IsAny<Action?>(), It.IsAny<Action?>(), It.IsAny<int>()))
            .Returns(mainDomRegistered);

        var syncBootStateAccessorMock = new Mock<ISyncBootStateAccessor>();
        syncBootStateAccessorMock.Setup(x => x.GetSyncBootState()).Returns(SyncBootState.WarmBoot);

        var globalSettingsMock = new Mock<IOptionsMonitor<GlobalSettings>>();
        globalSettingsMock.SetupGet(x => x.CurrentValue).Returns(new GlobalSettings());

        var machineInfoFactoryMock = new Mock<IMachineInfoFactory>();
        machineInfoFactoryMock.Setup(x => x.GetLocalIdentity()).Returns("test-identity");

        // No available request cache, so remote deliveries are written immediately instead of batched.
        var requestCacheMock = new Mock<IRequestCache>();
        requestCacheMock.SetupGet(x => x.IsAvailable).Returns(false);

        return new BatchedDatabaseServerMessenger(
            _mainDomMock.Object,
            new CacheRefresherCollection(() => new[] { _cacheRefresherMock.Object }),
            _loggerMock.Object,
            syncBootStateAccessorMock.Object,
            Mock.Of<IHostingEnvironment>(),
            _cacheInstructionServiceMock.Object,
            Mock.Of<IJsonSerializer>(),
            requestCacheMock.Object,
            Mock.Of<ILastSyncedManager>(),
            globalSettingsMock.Object,
            machineInfoFactoryMock.Object);
    }
}
