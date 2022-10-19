using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Scoping
{
    [TestFixture]
    public class ScopeUnitTests
    {
        /// <summary>
        /// Creates a ScopeProvider with mocked internals.
        /// </summary>
        /// <param name="syntaxProviderMock">The mock of the ISqlSyntaxProvider2, used to count method calls.</param>
        /// <returns></returns>
        private ScopeProvider GetScopeProvider(out Mock<IDistributedLockingMechanism> lockingMechanism)
        {
            var loggerFactory = NullLoggerFactory.Instance;
            var fileSystems = new FileSystems(
                loggerFactory,
                Mock.Of<IIOHelper>(),
                Mock.Of<IOptions<GlobalSettings>>(),
                Mock.Of<IHostingEnvironment>());
            var mediaFileManager = new MediaFileManager(
                Mock.Of<IFileSystem>(),
                Mock.Of<IMediaPathScheme>(),
                loggerFactory.CreateLogger<MediaFileManager>(),
                Mock.Of<IShortStringHelper>(),
                Mock.Of<IServiceProvider>(),
                Options.Create(new ContentSettings()));
            var databaseFactory = new Mock<IUmbracoDatabaseFactory>();
            var database = new Mock<IUmbracoDatabase>();
            var sqlContext = new Mock<ISqlContext>();

            lockingMechanism = new Mock<IDistributedLockingMechanism>();
            lockingMechanism.Setup(x => x.ReadLock(It.IsAny<int>(), It.IsAny<TimeSpan?>()))
                .Returns(Mock.Of<IDistributedLock>());
            lockingMechanism.Setup(x => x.WriteLock(It.IsAny<int>(), It.IsAny<TimeSpan?>()))
                .Returns(Mock.Of<IDistributedLock>());

            var lockingMechanismFactory = new Mock<IDistributedLockingMechanismFactory>();
            lockingMechanismFactory.Setup(x => x.DistributedLockingMechanism)
                .Returns(lockingMechanism.Object);

            // Setup mock of database factory to return mock of database.
            databaseFactory.Setup(x => x.CreateDatabase()).Returns(database.Object);
            databaseFactory.Setup(x => x.SqlContext).Returns(sqlContext.Object);

            // Setup mock of database to return mock of sql SqlContext
            database.Setup(x => x.SqlContext).Returns(sqlContext.Object);

            var syntaxProviderMock = new Mock<ISqlSyntaxProvider>();

            // Setup mock of ISqlContext to return syntaxProviderMock
            sqlContext.Setup(x => x.SqlSyntax).Returns(syntaxProviderMock.Object);

            return new ScopeProvider(
                new AmbientScopeStack(),
                new AmbientScopeContextStack(),
                lockingMechanismFactory.Object,
                databaseFactory.Object,
                fileSystems,
                new TestOptionsMonitor<CoreDebugSettings>(new CoreDebugSettings()),
                mediaFileManager,
                loggerFactory,
                Mock.Of<IEventAggregator>());
        }

        [Test]
        public void Unused_Lazy_Locks_Cleared_At_Child_Scope()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            var outerScope = scopeProvider.CreateScope();
            outerScope.ReadLock(Constants.Locks.Domains);
            outerScope.ReadLock(Constants.Locks.Languages);

            using (var innerScope1 = (Scope)scopeProvider.CreateScope())
            {
                innerScope1.ReadLock(Constants.Locks.Domains);
                innerScope1.ReadLock(Constants.Locks.Languages);

                innerScope1.Complete();
            }

            using (var innerScope2 = (Scope)scopeProvider.CreateScope())
            {
                innerScope2.ReadLock(Constants.Locks.Domains);
                innerScope2.ReadLock(Constants.Locks.Languages);

                // force resolving the locks
                var locks = innerScope2.GetReadLocks();

                innerScope2.Complete();
            }

            outerScope.Complete();

            Assert.DoesNotThrow(() => outerScope.Dispose());
        }

        [Test]
        public void WriteLock_Acquired_Only_Once_Per_Key()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var outerScope = (Scope)scopeProvider.CreateScope())
            {
                outerScope.EagerWriteLock(Constants.Locks.Domains);
                outerScope.EagerWriteLock(Constants.Locks.Languages);

                using (var innerScope1 = (Scope)scopeProvider.CreateScope())
                {
                    innerScope1.EagerWriteLock(Constants.Locks.Domains);
                    innerScope1.EagerWriteLock(Constants.Locks.Languages);

                    using (var innerScope2 = (Scope)scopeProvider.CreateScope())
                    {
                        innerScope2.EagerWriteLock(Constants.Locks.Domains);
                        innerScope2.EagerWriteLock(Constants.Locks.Languages);
                        innerScope2.Complete();
                    }

                    innerScope1.Complete();
                }

                outerScope.Complete();
            }

            syntaxProviderMock.Verify(x => x.WriteLock(Constants.Locks.Domains, It.IsAny<TimeSpan?>()), Times.Once);
            syntaxProviderMock.Verify(x => x.WriteLock(Constants.Locks.Languages, It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Test]
        public void WriteLock_Acquired_Only_Once_When_InnerScope_Disposed()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var outerScope = (Scope)scopeProvider.CreateScope())
            {
                outerScope.EagerWriteLock(Constants.Locks.Languages);

                using (var innerScope = (Scope)scopeProvider.CreateScope())
                {
                    innerScope.EagerWriteLock(Constants.Locks.Languages);
                    innerScope.EagerWriteLock(Constants.Locks.ContentTree);
                    innerScope.Complete();
                }

                outerScope.EagerWriteLock(Constants.Locks.ContentTree);
                outerScope.Complete();
            }

            syntaxProviderMock.Verify(x => x.WriteLock(Constants.Locks.Languages, It.IsAny<TimeSpan?>()), Times.Once);
            syntaxProviderMock.Verify(x => x.WriteLock(Constants.Locks.ContentTree, It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Test]
        public void WriteLock_With_Timeout_Acquired_Only_Once_Per_Key()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            var timeout = TimeSpan.FromMilliseconds(10000);

            using (var outerScope = (Scope)scopeProvider.CreateScope())
            {
                outerScope.EagerWriteLock(timeout, Constants.Locks.Domains);
                outerScope.EagerWriteLock(timeout, Constants.Locks.Languages);

                using (var innerScope1 = (Scope)scopeProvider.CreateScope())
                {
                    innerScope1.EagerWriteLock(timeout, Constants.Locks.Domains);
                    innerScope1.EagerWriteLock(timeout, Constants.Locks.Languages);

                    using (var innerScope2 = (Scope)scopeProvider.CreateScope())
                    {
                        innerScope2.EagerWriteLock(timeout, Constants.Locks.Domains);
                        innerScope2.EagerWriteLock(timeout, Constants.Locks.Languages);
                        innerScope2.Complete();
                    }

                    innerScope1.Complete();
                }

                outerScope.Complete();
            }

            syntaxProviderMock.Verify(x => x.WriteLock(Constants.Locks.Domains, It.IsAny<TimeSpan?>()), Times.Once);
            syntaxProviderMock.Verify(x => x.WriteLock(Constants.Locks.Languages, It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Test]
        public void ReadLock_Acquired_Only_Once_Per_Key()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var outerScope = (Scope)scopeProvider.CreateScope())
            {
                outerScope.EagerReadLock(Constants.Locks.Domains);
                outerScope.EagerReadLock(Constants.Locks.Languages);

                using (var innerScope1 = (Scope)scopeProvider.CreateScope())
                {
                    innerScope1.EagerReadLock(Constants.Locks.Domains);
                    innerScope1.EagerReadLock(Constants.Locks.Languages);

                    using (var innerScope2 = (Scope)scopeProvider.CreateScope())
                    {
                        innerScope2.EagerReadLock(Constants.Locks.Domains);
                        innerScope2.EagerReadLock(Constants.Locks.Languages);

                        innerScope2.Complete();
                    }

                    innerScope1.Complete();
                }

                outerScope.Complete();
            }

            syntaxProviderMock.Verify(x => x.ReadLock(Constants.Locks.Domains, It.IsAny<TimeSpan?>()), Times.Once);
            syntaxProviderMock.Verify(x => x.ReadLock(Constants.Locks.Languages, It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Test]
        public void ReadLock_With_Timeout_Acquired_Only_Once_Per_Key()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            var timeOut = TimeSpan.FromMilliseconds(10000);

            using (var outerScope = (Scope)scopeProvider.CreateScope())
            {
                outerScope.EagerReadLock(timeOut, Constants.Locks.Domains);
                outerScope.EagerReadLock(timeOut, Constants.Locks.Languages);

                using (var innerScope1 = (Scope)scopeProvider.CreateScope())
                {
                    innerScope1.EagerReadLock(timeOut, Constants.Locks.Domains);
                    innerScope1.EagerReadLock(timeOut, Constants.Locks.Languages);

                    using (var innerScope2 = (Scope)scopeProvider.CreateScope())
                    {
                        innerScope2.EagerReadLock(timeOut, Constants.Locks.Domains);
                        innerScope2.EagerReadLock(timeOut, Constants.Locks.Languages);

                        innerScope2.Complete();
                    }

                    innerScope1.Complete();
                }

                outerScope.Complete();
            }

            syntaxProviderMock.Verify(x => x.ReadLock(Constants.Locks.Domains, It.IsAny<TimeSpan?>()), Times.Once);
            syntaxProviderMock.Verify(x => x.ReadLock(Constants.Locks.Languages, It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Test]
        public void ReadLock_Acquired_Only_Once_When_InnerScope_Disposed()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var outerScope = scopeProvider.CreateScope())
            {
                outerScope.ReadLock(Constants.Locks.Languages);

                using (var innerScope = (Scope)scopeProvider.CreateScope())
                {
                    innerScope.EagerReadLock(Constants.Locks.Languages);
                    innerScope.EagerReadLock(Constants.Locks.ContentTree);
                    innerScope.Complete();
                }

                outerScope.ReadLock(Constants.Locks.ContentTree);
                outerScope.Complete();
            }

            syntaxProviderMock.Verify(x => x.ReadLock(Constants.Locks.Languages, It.IsAny<TimeSpan?>()), Times.Once);
            syntaxProviderMock.Verify(x => x.ReadLock(Constants.Locks.ContentTree, It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Test]
        public void WriteLocks_Count_correctly_If_Lock_Requested_Twice_In_Scope()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            Guid innerscopeId;

            using (var outerscope = (Scope)scopeProvider.CreateScope())
            {
                outerscope.EagerWriteLock(Constants.Locks.ContentTree);
                outerscope.EagerWriteLock(Constants.Locks.ContentTree);
                Assert.AreEqual(2, outerscope.GetWriteLocks()[outerscope.InstanceId][Constants.Locks.ContentTree]);

                using (var innerScope = (Scope)scopeProvider.CreateScope())
                {
                    innerscopeId = innerScope.InstanceId;
                    innerScope.EagerWriteLock(Constants.Locks.ContentTree);
                    innerScope.EagerWriteLock(Constants.Locks.ContentTree);
                    Assert.AreEqual(2, outerscope.GetWriteLocks()[outerscope.InstanceId][Constants.Locks.ContentTree]);
                    Assert.AreEqual(2, outerscope.GetWriteLocks()[innerscopeId][Constants.Locks.ContentTree]);

                    innerScope.EagerWriteLock(Constants.Locks.Languages);
                    innerScope.EagerWriteLock(Constants.Locks.Languages);
                    Assert.AreEqual(2, outerscope.GetWriteLocks()[innerScope.InstanceId][Constants.Locks.Languages]);
                    innerScope.Complete();
                }

                Assert.AreEqual(2, outerscope.GetWriteLocks()[outerscope.InstanceId][Constants.Locks.ContentTree]);
                Assert.IsFalse(outerscope.GetWriteLocks().ContainsKey(innerscopeId));
                outerscope.Complete();
            }
        }

        [Test]
        public void ReadLocks_Count_correctly_If_Lock_Requested_Twice_In_Scope()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            Guid innerscopeId;

            using (var outerscope = (Scope)scopeProvider.CreateScope())
            {
                outerscope.EagerReadLock(Constants.Locks.ContentTree);
                outerscope.EagerReadLock(Constants.Locks.ContentTree);
                Assert.AreEqual(2, outerscope.GetReadLocks()[outerscope.InstanceId][Constants.Locks.ContentTree]);

                using (var innerScope = (Scope)scopeProvider.CreateScope())
                {
                    innerscopeId = innerScope.InstanceId;
                    innerScope.EagerReadLock(Constants.Locks.ContentTree);
                    innerScope.EagerReadLock(Constants.Locks.ContentTree);
                    Assert.AreEqual(2, outerscope.GetReadLocks()[outerscope.InstanceId][Constants.Locks.ContentTree]);
                    Assert.AreEqual(2, outerscope.GetReadLocks()[innerScope.InstanceId][Constants.Locks.ContentTree]);

                    innerScope.EagerReadLock(Constants.Locks.Languages);
                    innerScope.EagerReadLock(Constants.Locks.Languages);
                    Assert.AreEqual(2, outerscope.GetReadLocks()[innerScope.InstanceId][Constants.Locks.Languages]);
                    innerScope.Complete();
                }

                Assert.AreEqual(2, outerscope.GetReadLocks()[outerscope.InstanceId][Constants.Locks.ContentTree]);
                Assert.IsFalse(outerscope.GetReadLocks().ContainsKey(innerscopeId));

                outerscope.Complete();
            }
        }

        [Test]
        public void Nested_Scopes_WriteLocks_Count_Correctly()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            Guid innerScope1Id, innerScope2Id;

            using (var parentScope = scopeProvider.CreateScope())
            {
                var realParentScope = (Scope)parentScope;
                parentScope.WriteLock(Constants.Locks.ContentTree);
                parentScope.WriteLock(Constants.Locks.ContentTypes);

                Assert.AreEqual(1, realParentScope.GetWriteLocks()[realParentScope.InstanceId][Constants.Locks.ContentTree], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, realParentScope.GetWriteLocks()[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTypes)}");

                using (var innerScope1 = scopeProvider.CreateScope())
                {
                    innerScope1Id = innerScope1.InstanceId;
                    innerScope1.WriteLock(Constants.Locks.ContentTree);
                    innerScope1.WriteLock(Constants.Locks.ContentTypes);
                    innerScope1.WriteLock(Constants.Locks.Languages);

                    Assert.AreEqual(1, realParentScope.GetWriteLocks()[realParentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope1, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.GetWriteLocks()[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.GetWriteLocks()[innerScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.GetWriteLocks()[innerScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.GetWriteLocks()[innerScope1.InstanceId][Constants.Locks.Languages], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.Languages)}");

                    using (var innerScope2 = scopeProvider.CreateScope())
                    {
                        innerScope2Id = innerScope2.InstanceId;
                        innerScope2.WriteLock(Constants.Locks.ContentTree);
                        innerScope2.WriteLock(Constants.Locks.MediaTypes);

                        Assert.AreEqual(1, realParentScope.GetWriteLocks()[realParentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope2, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, realParentScope.GetWriteLocks()[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope2, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, realParentScope.GetWriteLocks()[innerScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, realParentScope.GetWriteLocks()[innerScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, realParentScope.GetWriteLocks()[innerScope1.InstanceId][Constants.Locks.Languages], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.Languages)}");
                        Assert.AreEqual(1, realParentScope.GetWriteLocks()[innerScope2.InstanceId][Constants.Locks.ContentTree], $"innerScope2, innerScope2 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, realParentScope.GetWriteLocks()[innerScope2.InstanceId][Constants.Locks.MediaTypes], $"innerScope2, innerScope2 instance, after locks acquired: {nameof(Constants.Locks.MediaTypes)}");

                        innerScope2.Complete();
                    }

                    Assert.AreEqual(1, realParentScope.GetWriteLocks()[realParentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope1, parent instance, after innserScope2 disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.GetWriteLocks()[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, parent instance, after innserScope2 disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.GetWriteLocks()[innerScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope1, innerScope1 instance, after innserScope2 disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.GetWriteLocks()[innerScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, innerScope1 instance, after innserScope2 disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.GetWriteLocks()[innerScope1.InstanceId][Constants.Locks.Languages], $"innerScope1, innerScope1 instance, after innserScope2 disposed: {nameof(Constants.Locks.Languages)}");
                    Assert.IsFalse(realParentScope.GetWriteLocks().ContainsKey(innerScope2Id));

                    innerScope1.Complete();
                }

                Assert.AreEqual(1, realParentScope.GetWriteLocks()[realParentScope.InstanceId][Constants.Locks.ContentTree], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, realParentScope.GetWriteLocks()[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.ContentTypes)}");
                Assert.IsFalse(realParentScope.GetWriteLocks().ContainsKey(innerScope2Id));
                Assert.IsFalse(realParentScope.GetWriteLocks().ContainsKey(innerScope1Id));

                parentScope.Complete();
            }
        }

        [Test]
        public void Nested_Scopes_ReadLocks_Count_Correctly()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            Guid innerScope1Id, innerScope2Id;

            using (var parentScope = scopeProvider.CreateScope())
            {
                var realParentScope = (Scope)parentScope;
                parentScope.ReadLock(Constants.Locks.ContentTree);
                parentScope.ReadLock(Constants.Locks.ContentTypes);
                Assert.AreEqual(1, realParentScope.GetReadLocks()[realParentScope.InstanceId][Constants.Locks.ContentTree], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, realParentScope.GetReadLocks()[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTypes)}");

                using (var innserScope1 = scopeProvider.CreateScope())
                {
                    innerScope1Id = innserScope1.InstanceId;
                    innserScope1.ReadLock(Constants.Locks.ContentTree);
                    innserScope1.ReadLock(Constants.Locks.ContentTypes);
                    innserScope1.ReadLock(Constants.Locks.Languages);
                    Assert.AreEqual(1, realParentScope.GetReadLocks()[realParentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope1, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.GetReadLocks()[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.GetReadLocks()[innserScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.GetReadLocks()[innserScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.GetReadLocks()[innserScope1.InstanceId][Constants.Locks.Languages], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.Languages)}");

                    using (var innerScope2 = scopeProvider.CreateScope())
                    {
                        innerScope2Id = innerScope2.InstanceId;
                        innerScope2.ReadLock(Constants.Locks.ContentTree);
                        innerScope2.ReadLock(Constants.Locks.MediaTypes);
                        Assert.AreEqual(1, realParentScope.GetReadLocks()[realParentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope2, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, realParentScope.GetReadLocks()[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope2, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, realParentScope.GetReadLocks()[innserScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, realParentScope.GetReadLocks()[innserScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, realParentScope.GetReadLocks()[innserScope1.InstanceId][Constants.Locks.Languages], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.Languages)}");
                        Assert.AreEqual(1, realParentScope.GetReadLocks()[innerScope2.InstanceId][Constants.Locks.ContentTree], $"innerScope2, innerScope2 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, realParentScope.GetReadLocks()[innerScope2.InstanceId][Constants.Locks.MediaTypes], $"innerScope2, innerScope2 instance, after locks acquired: {nameof(Constants.Locks.MediaTypes)}");

                        innerScope2.Complete();
                    }

                    Assert.AreEqual(1, realParentScope.GetReadLocks()[realParentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope1, parent instance, after innerScope2 disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.GetReadLocks()[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, parent instance, after innerScope2 disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.GetReadLocks()[innserScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope1, innerScope1 instance, after innerScope2 disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.GetReadLocks()[innserScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, innerScope1 instance, after innerScope2 disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.GetReadLocks()[innserScope1.InstanceId][Constants.Locks.Languages], $"innerScope1, innerScope1 instance, after innerScope2 disposed: {nameof(Constants.Locks.Languages)}");
                    Assert.IsFalse(realParentScope.GetReadLocks().ContainsKey(innerScope2Id));

                    innserScope1.Complete();
                }

                Assert.AreEqual(1, realParentScope.GetReadLocks()[realParentScope.InstanceId][Constants.Locks.ContentTree], $"parentScope after innerScope1 disposed: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, realParentScope.GetReadLocks()[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"parentScope after innerScope1 disposed: {nameof(Constants.Locks.ContentTypes)}");
                Assert.IsFalse(realParentScope.GetReadLocks().ContainsKey(innerScope2Id));
                Assert.IsFalse(realParentScope.GetReadLocks().ContainsKey(innerScope1Id));

                parentScope.Complete();
            }
        }

        [Test]
        public void WriteLock_Doesnt_Increment_On_Error()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            syntaxProviderMock.Setup(x => x.WriteLock(It.IsAny<int>(), It.IsAny<TimeSpan?>())).Throws(new Exception("Boom"));

            using (var scope = (Scope)scopeProvider.CreateScope())
            {
                Assert.Throws<Exception>(() => scope.EagerWriteLock(Constants.Locks.Languages));
                Assert.IsFalse(scope.GetWriteLocks()[scope.InstanceId].ContainsKey(Constants.Locks.Languages));
                scope.Complete();
            }
        }

        [Test]
        public void ReadLock_Doesnt_Increment_On_Error()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            syntaxProviderMock.Setup(x => x.ReadLock(It.IsAny<int>(), It.IsAny<TimeSpan?>())).Throws(new Exception("Boom"));

            using (var scope = (Scope)scopeProvider.CreateScope())
            {
                Assert.Throws<Exception>(() => scope.EagerReadLock(Constants.Locks.Languages));
                Assert.IsFalse(scope.GetReadLocks()[scope.InstanceId].ContainsKey(Constants.Locks.Languages));
                scope.Complete();
            }
        }

        [Test]
        public void Scope_Throws_If_ReadLocks_Not_Cleared()
        {
            var scopeprovider = GetScopeProvider(out var syntaxProviderMock);
            var scope = (Scope)scopeprovider.CreateScope();

            try
            {
                // Request a lock to create the ReadLocks dict.
                scope.ReadLock(Constants.Locks.Domains);

                var readDict = new Dictionary<int, int>();
                readDict[Constants.Locks.Languages] = 1;
                scope.GetReadLocks()[Guid.NewGuid()] = readDict;

                Assert.Throws<InvalidOperationException>(() => scope.Dispose());
            }
            finally
            {
                // We have to clear so we can properly dispose the scope, otherwise it'll mess with other tests.
                scope.GetReadLocks()?.Clear();
                scope.Dispose();
            }
        }

        [Test]
        public void Scope_Throws_If_WriteLocks_Not_Cleared()
        {
            var scopeprovider = GetScopeProvider(out var syntaxProviderMock);
            var scope = (Scope)scopeprovider.CreateScope();

            try
            {
                // Request a lock to create the WriteLocks dict.
                scope.WriteLock(Constants.Locks.Domains);

                var writeDict = new Dictionary<int, int>();
                writeDict[Constants.Locks.Languages] = 1;
                scope.GetWriteLocks()[Guid.NewGuid()] = writeDict;

                Assert.Throws<InvalidOperationException>(() => scope.Dispose());
            }
            finally
            {
                // We have to clear so we can properly dispose the scope, otherwise it'll mess with other tests.
                scope.GetWriteLocks()?.Clear();
                scope.Dispose();
            }
        }

        [Test]
        public void WriteLocks_Not_Created_Until_First_Lock()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var scope = scopeProvider.CreateScope())
            {
                var realScope = (Scope)scope;
                Assert.IsNull(realScope.GetWriteLocks());
            }
        }

        [Test]
        public void ReadLocks_Not_Created_Until_First_Lock()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var scope = scopeProvider.CreateScope())
            {
                var realScope = (Scope)scope;
                Assert.IsNull(realScope.GetReadLocks());
            }
        }

        [Test]
        public void Depth_WhenRootScope_ReturnsZero()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var scope = scopeProvider.CreateScope())
            {
                Assert.AreEqual(0,scope.Depth);
            }
        }


        [Test]
        public void Depth_WhenChildScope_ReturnsDepth()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (scopeProvider.CreateScope())
            {
                using (scopeProvider.CreateScope())
                {
                    using (var c2 = scopeProvider.CreateScope())
                    {
                        Assert.AreEqual(2, c2.Depth);
                    }
                }
            }
        }
    }
}
