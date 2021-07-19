using System;
using System.Collections.Generic;
using System.Data;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    public class ScopeUnitTests
    {
        /// <summary>
        /// Creates a ScopeProvider with mocked internals.
        /// </summary>
        /// <param name="syntaxProviderMock">The mock of the ISqlSyntaxProvider2, used to count method calls.</param>
        /// <returns></returns>
        private ScopeProvider GetScopeProvider(out Mock<ISqlSyntaxProvider2> syntaxProviderMock)
        {
            var logger = Mock.Of<ILogger>();
            var fac = Mock.Of<IFactory>();
            var fileSystem = new FileSystems(fac, logger);
            var databaseFactory = new Mock<IUmbracoDatabaseFactory>();
            var database = new Mock<IUmbracoDatabase>();
            var sqlContext = new Mock<ISqlContext>();
            syntaxProviderMock = new Mock<ISqlSyntaxProvider2>();

            // Setup mock of database factory to return mock of database.
            databaseFactory.Setup(x => x.CreateDatabase()).Returns(database.Object);
            databaseFactory.Setup(x => x.SqlContext).Returns(sqlContext.Object);

            // Setup mock of database to return mock of sql SqlContext
            database.Setup(x => x.SqlContext).Returns(sqlContext.Object);

            // Setup syntax provider mock to return isolation level
            syntaxProviderMock.Setup(x => x.DefaultIsolationLevel).Returns(IsolationLevel.Unspecified);

            // Setup mock of ISqlContext to return syntaxProviderMock
            sqlContext.Setup(x => x.SqlSyntax).Returns(syntaxProviderMock.Object);

            return new ScopeProvider(databaseFactory.Object, fileSystem, logger);
        }

        [Test]
        public void WriteLock_Acquired_Only_Once_Per_Key()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var outerScope = (Scope) scopeProvider.CreateScope())
            {
                outerScope.EagerWriteLock(Constants.Locks.Domains);
                outerScope.EagerWriteLock(Constants.Locks.Languages);

                using (var innerScope1 = (Scope) scopeProvider.CreateScope())
                {
                    innerScope1.EagerWriteLock(Constants.Locks.Domains);
                    innerScope1.EagerWriteLock(Constants.Locks.Languages);

                    using (var innerScope2 = (Scope) scopeProvider.CreateScope())
                    {
                        innerScope2.EagerWriteLock(Constants.Locks.Domains);
                        innerScope2.EagerWriteLock(Constants.Locks.Languages);
                        innerScope2.Complete();
                    }
                    innerScope1.Complete();
                }
                outerScope.Complete();
            }

            syntaxProviderMock.Verify(x => x.WriteLock(It.IsAny<IDatabase>(), Constants.Locks.Domains), Times.Once);
            syntaxProviderMock.Verify(x => x.WriteLock(It.IsAny<IDatabase>(), Constants.Locks.Languages), Times.Once);
        }

        [Test]
        public void WriteLock_Acquired_Only_Once_When_InnerScope_Disposed()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var outerScope = (Scope) scopeProvider.CreateScope())
            {
                outerScope.EagerWriteLock(Constants.Locks.Languages);

                using (var innerScope = (Scope) scopeProvider.CreateScope())
                {
                    innerScope.EagerWriteLock(Constants.Locks.Languages);
                    innerScope.EagerWriteLock(Constants.Locks.ContentTree);
                    innerScope.Complete();
                }

                outerScope.EagerWriteLock(Constants.Locks.ContentTree);
                outerScope.Complete();
            }

            syntaxProviderMock.Verify(x => x.WriteLock(It.IsAny<IDatabase>(), Constants.Locks.Languages), Times.Once);
            syntaxProviderMock.Verify(x => x.WriteLock(It.IsAny<IDatabase>(), Constants.Locks.ContentTree), Times.Once);
        }

        [Test]
        public void WriteLock_With_Timeout_Acquired_Only_Once_Per_Key(){
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            var timeout = TimeSpan.FromMilliseconds(10000);

            using (var outerScope = (Scope) scopeProvider.CreateScope())
            {
                outerScope.EagerWriteLock(timeout, Constants.Locks.Domains);
                outerScope.EagerWriteLock(timeout, Constants.Locks.Languages);

                using (var innerScope1 = (Scope) scopeProvider.CreateScope())
                {
                    innerScope1.EagerWriteLock(timeout, Constants.Locks.Domains);
                    innerScope1.EagerWriteLock(timeout, Constants.Locks.Languages);

                    using (var innerScope2 = (Scope) scopeProvider.CreateScope())
                    {
                        innerScope2.EagerWriteLock(timeout, Constants.Locks.Domains);
                        innerScope2.EagerWriteLock(timeout, Constants.Locks.Languages);
                        innerScope2.Complete();
                    }
                    innerScope1.Complete();
                }

                outerScope.Complete();
            }

            syntaxProviderMock.Verify(x => x.WriteLock(It.IsAny<IDatabase>(), timeout, Constants.Locks.Domains), Times.Once);
            syntaxProviderMock.Verify(x => x.WriteLock(It.IsAny<IDatabase>(), timeout, Constants.Locks.Languages), Times.Once);
        }

        [Test]
        public void ReadLock_Acquired_Only_Once_Per_Key()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var outerScope = (Scope) scopeProvider.CreateScope())
            {
                outerScope.EagerReadLock(Constants.Locks.Domains);
                outerScope.EagerReadLock(Constants.Locks.Languages);

                using (var innerScope1 = (Scope) scopeProvider.CreateScope())
                {
                    innerScope1.EagerReadLock(Constants.Locks.Domains);
                    innerScope1.EagerReadLock(Constants.Locks.Languages);

                    using (var innerScope2 = (Scope) scopeProvider.CreateScope())
                    {
                        innerScope2.EagerReadLock(Constants.Locks.Domains);
                        innerScope2.EagerReadLock(Constants.Locks.Languages);

                        innerScope2.Complete();
                    }

                    innerScope1.Complete();
                }

                outerScope.Complete();
            }

            syntaxProviderMock.Verify(x => x.ReadLock(It.IsAny<IDatabase>(), Constants.Locks.Domains), Times.Once);
            syntaxProviderMock.Verify(x => x.ReadLock(It.IsAny<IDatabase>(), Constants.Locks.Languages), Times.Once);
        }

        [Test]
        public void ReadLock_With_Timeout_Acquired_Only_Once_Per_Key()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            var timeOut = TimeSpan.FromMilliseconds(10000);

            using (var outerScope = (Scope) scopeProvider.CreateScope())
            {
                outerScope.EagerReadLock(timeOut, Constants.Locks.Domains);
                outerScope.EagerReadLock(timeOut, Constants.Locks.Languages);

                using (var innerScope1 = (Scope) scopeProvider.CreateScope())
                {
                    innerScope1.EagerReadLock(timeOut, Constants.Locks.Domains);
                    innerScope1.EagerReadLock(timeOut, Constants.Locks.Languages);

                    using (var innerScope2 = (Scope) scopeProvider.CreateScope())
                    {
                        innerScope2.EagerReadLock(timeOut, Constants.Locks.Domains);
                        innerScope2.EagerReadLock(timeOut, Constants.Locks.Languages);

                        innerScope2.Complete();
                    }

                    innerScope1.Complete();
                }

                outerScope.Complete();
            }

            syntaxProviderMock.Verify(x => x.ReadLock(It.IsAny<IDatabase>(), timeOut, Constants.Locks.Domains), Times.Once);
            syntaxProviderMock.Verify(x => x.ReadLock(It.IsAny<IDatabase>(), timeOut, Constants.Locks.Languages), Times.Once);
        }

        [Test]
        public void ReadLock_Acquired_Only_Once_When_InnerScope_Disposed()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var outerScope = (Scope) scopeProvider.CreateScope())
            {
                outerScope.EagerReadLock(Constants.Locks.Languages);

                using (var innerScope = (Scope)scopeProvider.CreateScope())
                {
                    innerScope.EagerReadLock(Constants.Locks.Languages);
                    innerScope.EagerReadLock(Constants.Locks.ContentTree);
                    innerScope.Complete();
                }

                outerScope.EagerReadLock(Constants.Locks.ContentTree);
                outerScope.Complete();
            }

            syntaxProviderMock.Verify(x => x.ReadLock(It.IsAny<IDatabase>(), Constants.Locks.Languages), Times.Once);
            syntaxProviderMock.Verify(x => x.ReadLock(It.IsAny<IDatabase>(), Constants.Locks.ContentTree), Times.Once);
        }

        [Test]
        public void WriteLocks_Count_correctly_If_Lock_Requested_Twice_In_Scope()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            Guid innerscopeId;

            using (var outerScope = (Scope) scopeProvider.CreateScope())
            {
                outerScope.EagerWriteLock(Constants.Locks.ContentTree);
                outerScope.EagerWriteLock(Constants.Locks.ContentTree);
                Assert.AreEqual(2, outerScope.WriteLocks[outerScope.InstanceId][Constants.Locks.ContentTree]);

                using (var innerScope = (Scope) scopeProvider.CreateScope())
                {
                    innerscopeId = innerScope.InstanceId;
                    innerScope.EagerWriteLock(Constants.Locks.ContentTree);
                    innerScope.EagerWriteLock(Constants.Locks.ContentTree);
                    Assert.AreEqual(2, outerScope.WriteLocks[outerScope.InstanceId][Constants.Locks.ContentTree]);
                    Assert.AreEqual(2, outerScope.WriteLocks[innerscopeId][Constants.Locks.ContentTree]);

                    innerScope.EagerWriteLock(Constants.Locks.Languages);
                    innerScope.EagerWriteLock(Constants.Locks.Languages);
                    Assert.AreEqual(2, outerScope.WriteLocks[innerScope.InstanceId][Constants.Locks.Languages]);
                    innerScope.Complete();
                }
                Assert.AreEqual(2, outerScope.WriteLocks[outerScope.InstanceId][Constants.Locks.ContentTree]);
                Assert.IsFalse(outerScope.WriteLocks.ContainsKey(innerscopeId));
                outerScope.Complete();
            }
        }

        [Test]
        public void ReadLocks_Count_correctly_If_Lock_Requested_Twice_In_Scope()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            Guid innerscopeId;

            using (var outerScope = (Scope) scopeProvider.CreateScope())
            {
                outerScope.EagerReadLock(Constants.Locks.ContentTree);
                outerScope.EagerReadLock(Constants.Locks.ContentTree);
                Assert.AreEqual(2, outerScope.ReadLocks[outerScope.InstanceId][Constants.Locks.ContentTree]);

                using (var innerScope = (Scope) scopeProvider.CreateScope())
                {
                    innerscopeId = innerScope.InstanceId;
                    innerScope.EagerReadLock(Constants.Locks.ContentTree);
                    innerScope.EagerReadLock(Constants.Locks.ContentTree);
                    Assert.AreEqual(2, outerScope.ReadLocks[outerScope.InstanceId][Constants.Locks.ContentTree]);
                    Assert.AreEqual(2, outerScope.ReadLocks[innerScope.InstanceId][Constants.Locks.ContentTree]);

                    innerScope.EagerReadLock(Constants.Locks.Languages);
                    innerScope.EagerReadLock(Constants.Locks.Languages);
                    Assert.AreEqual(2, outerScope.ReadLocks[innerScope.InstanceId][Constants.Locks.Languages]);
                    innerScope.Complete();
                }
                Assert.AreEqual(2, outerScope.ReadLocks[outerScope.InstanceId][Constants.Locks.ContentTree]);
                Assert.IsFalse(outerScope.ReadLocks.ContainsKey(innerscopeId));


                outerScope.Complete();
            }
        }

        [Test]
        public void Nested_Scopes_WriteLocks_Count_Correctly()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            Guid innerScope1Id, innerScope2Id;

            using (var parentScope = (Scope) scopeProvider.CreateScope())
            {
                parentScope.EagerWriteLock(Constants.Locks.ContentTree);
                parentScope.EagerWriteLock(Constants.Locks.ContentTypes);

                Assert.AreEqual(1, parentScope.WriteLocks[parentScope.InstanceId][Constants.Locks.ContentTree], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, parentScope.WriteLocks[parentScope.InstanceId][Constants.Locks.ContentTypes], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTypes)}");

                using (var innerScope1 = (Scope)scopeProvider.CreateScope())
                {
                    innerScope1Id = innerScope1.InstanceId;
                    innerScope1.EagerWriteLock(Constants.Locks.ContentTree);
                    innerScope1.EagerWriteLock(Constants.Locks.ContentTypes);
                    innerScope1.EagerWriteLock(Constants.Locks.Languages);

                    Assert.AreEqual(1, parentScope.WriteLocks[parentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope1, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, parentScope.WriteLocks[parentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, parentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, parentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, parentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.Languages], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.Languages)}");

                    using (var innerScope2 = (Scope)scopeProvider.CreateScope())
                    {
                        innerScope2Id = innerScope2.InstanceId;
                        innerScope2.EagerWriteLock(Constants.Locks.ContentTree);
                        innerScope2.EagerWriteLock(Constants.Locks.MediaTypes);

                        Assert.AreEqual(1, parentScope.WriteLocks[parentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope2, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, parentScope.WriteLocks[parentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope2, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, parentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, parentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, parentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.Languages], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.Languages)}");
                        Assert.AreEqual(1, parentScope.WriteLocks[innerScope2.InstanceId][Constants.Locks.ContentTree], $"innerScope2, innerScope2 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, parentScope.WriteLocks[innerScope2.InstanceId][Constants.Locks.MediaTypes], $"innerScope2, innerScope2 instance, after locks acquired: {nameof(Constants.Locks.MediaTypes)}");

                        innerScope2.Complete();
                    }
                    Assert.AreEqual(1, parentScope.WriteLocks[parentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope1, parent instance, after innserScope2 disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, parentScope.WriteLocks[parentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, parent instance, after innserScope2 disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, parentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope1, innerScope1 instance, after innserScope2 disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, parentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, innerScope1 instance, after innserScope2 disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, parentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.Languages], $"innerScope1, innerScope1 instance, after innserScope2 disposed: {nameof(Constants.Locks.Languages)}");
                    Assert.IsFalse(parentScope.WriteLocks.ContainsKey(innerScope2Id));

                    innerScope1.Complete();
                }
                Assert.AreEqual(1, parentScope.WriteLocks[parentScope.InstanceId][Constants.Locks.ContentTree], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, parentScope.WriteLocks[parentScope.InstanceId][Constants.Locks.ContentTypes], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.ContentTypes)}");
                Assert.IsFalse(parentScope.WriteLocks.ContainsKey(innerScope2Id));
                Assert.IsFalse(parentScope.WriteLocks.ContainsKey(innerScope1Id));

                parentScope.Complete();
            }
        }

        [Test]
        public void Nested_Scopes_ReadLocks_Count_Correctly()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            Guid innerScope1Id, innerScope2Id;

            using (var parentScope = (Scope) scopeProvider.CreateScope())
            {
                parentScope.EagerReadLock(Constants.Locks.ContentTree);
                parentScope.EagerReadLock(Constants.Locks.ContentTypes);
                Assert.AreEqual(1, parentScope.ReadLocks[parentScope.InstanceId][Constants.Locks.ContentTree], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, parentScope.ReadLocks[parentScope.InstanceId][Constants.Locks.ContentTypes], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTypes)}");

                using (var innserScope1 = (Scope) scopeProvider.CreateScope())
                {
                    innerScope1Id = innserScope1.InstanceId;
                    innserScope1.EagerReadLock(Constants.Locks.ContentTree);
                    innserScope1.EagerReadLock(Constants.Locks.ContentTypes);
                    innserScope1.EagerReadLock(Constants.Locks.Languages);
                    Assert.AreEqual(1, parentScope.ReadLocks[parentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope1, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, parentScope.ReadLocks[parentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, parentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, parentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, parentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.Languages], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.Languages)}");

                    using (var innerScope2 = (Scope) scopeProvider.CreateScope())
                    {
                        innerScope2Id = innerScope2.InstanceId;
                        innerScope2.EagerReadLock(Constants.Locks.ContentTree);
                        innerScope2.EagerReadLock(Constants.Locks.MediaTypes);
                        Assert.AreEqual(1, parentScope.ReadLocks[parentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope2, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, parentScope.ReadLocks[parentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope2, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, parentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, parentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, parentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.Languages], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.Languages)}");
                        Assert.AreEqual(1, parentScope.ReadLocks[innerScope2.InstanceId][Constants.Locks.ContentTree], $"innerScope2, innerScope2 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, parentScope.ReadLocks[innerScope2.InstanceId][Constants.Locks.MediaTypes], $"innerScope2, innerScope2 instance, after locks acquired: {nameof(Constants.Locks.MediaTypes)}");

                        innerScope2.Complete();
                    }

                    Assert.AreEqual(1, parentScope.ReadLocks[parentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope1, parent instance, after innerScope2 disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, parentScope.ReadLocks[parentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, parent instance, after innerScope2 disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, parentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope1, innerScope1 instance, after innerScope2 disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, parentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, innerScope1 instance, after innerScope2 disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, parentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.Languages], $"innerScope1, innerScope1 instance, after innerScope2 disposed: {nameof(Constants.Locks.Languages)}");
                    Assert.IsFalse(parentScope.ReadLocks.ContainsKey(innerScope2Id));

                    innserScope1.Complete();
                }

                Assert.AreEqual(1, parentScope.ReadLocks[parentScope.InstanceId][Constants.Locks.ContentTree], $"parentScope after innerScope1 disposed: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, parentScope.ReadLocks[parentScope.InstanceId][Constants.Locks.ContentTypes], $"parentScope after innerScope1 disposed: {nameof(Constants.Locks.ContentTypes)}");
                Assert.IsFalse(parentScope.ReadLocks.ContainsKey(innerScope2Id));
                Assert.IsFalse(parentScope.ReadLocks.ContainsKey(innerScope1Id));

                parentScope.Complete();
            }
        }

        [Test]
        public void WriteLock_Doesnt_Increment_On_Error()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            syntaxProviderMock.Setup(x => x.WriteLock(It.IsAny<IDatabase>(), It.IsAny<int[]>())).Throws(new Exception("Boom"));

            using (var scope =  (Scope) scopeProvider.CreateScope())
            {
                Assert.Throws<Exception>(() => scope.EagerWriteLock(Constants.Locks.Languages));
                Assert.IsFalse(scope.WriteLocks[scope.InstanceId].ContainsKey(Constants.Locks.Languages));
                scope.Complete();
            }
        }

        [Test]
        public void ReadLock_Doesnt_Increment_On_Error()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            syntaxProviderMock.Setup(x => x.ReadLock(It.IsAny<IDatabase>(), It.IsAny<int[]>())).Throws(new Exception("Boom"));

            using (var scope = (Scope) scopeProvider.CreateScope())
            {
                Assert.Throws<Exception>(() => scope.EagerReadLock(Constants.Locks.Languages));
                Assert.IsFalse(scope.ReadLocks[scope.InstanceId].ContainsKey(Constants.Locks.Languages));
                scope.Complete();
            }
        }

        [Test]
        public void Scope_Throws_If_ReadLocks_Not_Cleared()
        {
            var scopeprovider = GetScopeProvider(out var syntaxProviderMock);
            var scope = (Scope) scopeprovider.CreateScope();

            try
            {
                // Request a lock to create the ReadLocks dict.
                scope.EagerReadLock(Constants.Locks.Domains);

                var readDict = new Dictionary<int, int>();
                readDict[Constants.Locks.Languages] = 1;
                scope.ReadLocks[Guid.NewGuid()] = readDict;

                Assert.Throws<InvalidOperationException>(() => scope.Dispose());
            }
            finally
            {
                // We have to clear so we can properly dispose the scope, otherwise it'll mess with other tests.
                scope.ReadLocks?.Clear();
                scope.Dispose();
            }
        }

        [Test]
        public void Scope_Throws_If_WriteLocks_Not_Cleared()
        {
            var scopeprovider = GetScopeProvider(out var syntaxProviderMock);
            var scope = (Scope) scopeprovider.CreateScope();

            try
            {
                // Request a lock to create the WriteLocks dict.
                scope.EagerWriteLock(Constants.Locks.Domains);

                var writeDict = new Dictionary<int, int>();
                writeDict[Constants.Locks.Languages] = 1;
                scope.WriteLocks[Guid.NewGuid()] = writeDict;

                Assert.Throws<InvalidOperationException>(() => scope.Dispose());
            }
            finally
            {
                // We have to clear so we can properly dispose the scope, otherwise it'll mess with other tests.
                scope.WriteLocks?.Clear();
                scope.Dispose();
            }
        }

        [Test]
        public void WriteLocks_Not_Created_Until_First_Lock()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var scope = (Scope) scopeProvider.CreateScope())
            {
                Assert.IsNull(scope.WriteLocks);
            }
        }

        [Test]
        public void ReadLocks_Not_Created_Until_First_Lock()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var scope = (Scope) scopeProvider.CreateScope())
            {
                Assert.IsNull(scope.ReadLocks);
            }
        }
    }
}
