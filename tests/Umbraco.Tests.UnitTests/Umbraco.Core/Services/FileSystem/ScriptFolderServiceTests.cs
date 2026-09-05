// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.FileSystem;

/// <summary>
///     Covers the folder name validation in <see cref="FolderServiceOperationBase{TRepository,TFolderModel,TOperationStatus}" />
///     through one of its concrete implementations.
/// </summary>
[TestFixture]
public class ScriptFolderServiceTests
{
    private Mock<IScriptRepository> _repository = null!;
    private ScriptFolderService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IScriptRepository>();
        _repository.Setup(x => x.FolderExists(It.IsAny<string>())).Returns(false);

        var scopeProvider = new Mock<ICoreScopeProvider>();
        scopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());

        _sut = new ScriptFolderService(_repository.Object, scopeProvider.Object);
    }

    [Test]
    public async Task Can_Create_Folder_With_Valid_Name()
    {
        Attempt<ScriptFolderModel?, ScriptFolderOperationStatus> result =
            await _sut.CreateAsync(new ScriptFolderCreateModel { Name = "valid-folder" });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ScriptFolderOperationStatus.Success, result.Status);
        _repository.Verify(x => x.AddFolder(It.IsAny<string>()), Times.Once);
    }

    // '/' is invalid in a file name on every platform, unlike ':' which is only invalid on Windows.
    [TestCase("in/valid")]
    [TestCase("/")]
    public async Task Cannot_Create_Folder_With_Invalid_Name(string name)
    {
        Attempt<ScriptFolderModel?, ScriptFolderOperationStatus> result =
            await _sut.CreateAsync(new ScriptFolderCreateModel { Name = name });

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ScriptFolderOperationStatus.InvalidName, result.Status);
        _repository.Verify(x => x.AddFolder(It.IsAny<string>()), Times.Never);
    }
}
