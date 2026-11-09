// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class FileServiceBaseTests
{
    private TestFileService _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new TestFileService(
        Mock.Of<ICoreScopeProvider>(),
        NullLoggerFactory.Instance,
        Mock.Of<IEventMessagesFactory>(),
        Mock.Of<IScriptRepository>());

    [TestCase("valid.js")]
    [TestCase("valid-name.js")]
    [TestCase("valid name.js")]
    public void Can_Accept_Valid_File_Name(string fileName)
        => Assert.IsTrue(_sut.HasValidFileNameForTest(fileName));

    // '/' is invalid in a file name on every platform, unlike ':' which is only invalid on Windows.
    [TestCase("in/valid.js")]
    [TestCase("/.js")]
    public void Cannot_Accept_File_Name_With_Invalid_Characters(string fileName)
        => Assert.IsFalse(_sut.HasValidFileNameForTest(fileName));

    private sealed class TestFileService : FileServiceBase<IScriptRepository, IScript>
    {
        public TestFileService(
            ICoreScopeProvider provider,
            ILoggerFactory loggerFactory,
            IEventMessagesFactory eventMessagesFactory,
            IScriptRepository repository)
            : base(provider, loggerFactory, eventMessagesFactory, repository)
        {
        }

        protected override string[] AllowedFileExtensions => [".js"];

        public bool HasValidFileNameForTest(string fileName) => HasValidFileName(fileName);
    }
}
