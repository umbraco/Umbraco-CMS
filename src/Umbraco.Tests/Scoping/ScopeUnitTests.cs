using System;
using System.Collections.Generic;
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

            // Setup mock of database to return mock of sql SqlContext
            database.Setup(x => x.SqlContext).Returns(sqlContext.Object);

            // Setup mock of ISqlContext to return syntaxProviderMock
            sqlContext.Setup(x => x.SqlSyntax).Returns(syntaxProviderMock.Object);

            return new ScopeProvider(databaseFactory.Object, fileSystem, logger);
        }

        [Test]
        public void WriteLock_Acquired_Only_Once_Per_Key()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var outerScope = scopeProvider.CreateScope())
            {
                outerScope.WriteLock(Constants.Locks.Domains);
                outerScope.WriteLock(Constants.Locks.Languages);

                using (var innerScope1 = scopeProvider.CreateScope())
                {
                    innerScope1.WriteLock(Constants.Locks.Domains);
                    innerScope1.WriteLock(Constants.Locks.Languages);

                    using (var innerScope2 = scopeProvider.CreateScope())
                    {
                        innerScope2.WriteLock(Constants.Locks.Domains);
                        innerScope2.WriteLock(Constants.Locks.Languages);
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

            using (var outerScope = scopeProvider.CreateScope())
            {
                outerScope.WriteLock(Constants.Locks.Languages);

                using (var innerScope = scopeProvider.CreateScope())
                {
                    innerScope.WriteLock(Constants.Locks.Languages);
                    innerScope.WriteLock(Constants.Locks.ContentTree);
                    innerScope.Complete();
                }

                outerScope.WriteLock(Constants.Locks.ContentTree);
                outerScope.Complete();
            }

            syntaxProviderMock.Verify(x => x.WriteLock(It.IsAny<IDatabase>(), Constants.Locks.Languages), Times.Once);
            syntaxProviderMock.Verify(x => x.WriteLock(It.IsAny<IDatabase>(), Constants.Locks.ContentTree), Times.Once);
        }

        [Test]
        public void WriteLock_With_Timeout_Acquired_Only_Once_Per_Key(){
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            var timeout = TimeSpan.FromMilliseconds(10000);

            using (var outerScope = scopeProvider.CreateScope())
            {
                var realScope = (Scope) outerScope;
                realScope.WriteLock(timeout, Constants.Locks.Domains);
                realScope.WriteLock(timeout, Constants.Locks.Languages);

                using (var innerScope1 = scopeProvider.CreateScope())
                {
                    var realInnerScope1 = (Scope) outerScope;
                    realInnerScope1.WriteLock(timeout, Constants.Locks.Domains);
                    realInnerScope1.WriteLock(timeout, Constants.Locks.Languages);

                    using (var innerScope2 = scopeProvider.CreateScope())
                    {
                        var realInnerScope2 = (Scope) innerScope2;
                        realInnerScope2.WriteLock(timeout, Constants.Locks.Domains);
                        realInnerScope2.WriteLock(timeout, Constants.Locks.Languages);
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

            using (var outerScope = scopeProvider.CreateScope())
            {
                outerScope.ReadLock(Constants.Locks.Domains);
                outerScope.ReadLock(Constants.Locks.Languages);

                using (var innerScope1 = scopeProvider.CreateScope())
                {
                    innerScope1.ReadLock(Constants.Locks.Domains);
                    innerScope1.ReadLock(Constants.Locks.Languages);

                    using (var innerScope2 = scopeProvider.CreateScope())
                    {
                        innerScope2.ReadLock(Constants.Locks.Domains);
                        innerScope2.ReadLock(Constants.Locks.Languages);

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

            using (var outerScope = scopeProvider.CreateScope())
            {
                var realOuterScope = (Scope) outerScope;
                realOuterScope.ReadLock(timeOut, Constants.Locks.Domains);
                realOuterScope.ReadLock(timeOut, Constants.Locks.Languages);

                using (var innerScope1 = scopeProvider.CreateScope())
                {
                    var realInnerScope1 = (Scope) innerScope1;
                    realInnerScope1.ReadLock(timeOut, Constants.Locks.Domains);
                    realInnerScope1.ReadLock(timeOut, Constants.Locks.Languages);

                    using (var innerScope2 = scopeProvider.CreateScope())
                    {
                        var realInnerScope2 = (Scope) innerScope2;
                        realInnerScope2.ReadLock(timeOut, Constants.Locks.Domains);
                        realInnerScope2.ReadLock(timeOut, Constants.Locks.Languages);

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

            using (var outerScope = scopeProvider.CreateScope())
            {
                outerScope.ReadLock(Constants.Locks.Languages);

                using (var innerScope = scopeProvider.CreateScope())
                {
                    innerScope.ReadLock(Constants.Locks.Languages);
                    innerScope.ReadLock(Constants.Locks.ContentTree);
                    innerScope.Complete();
                }

                outerScope.ReadLock(Constants.Locks.ContentTree);
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

            using (var outerscope = scopeProvider.CreateScope())
            {
                var realOuterScope = (Scope) outerscope;
                outerscope.WriteLock(Constants.Locks.ContentTree);
                outerscope.WriteLock(Constants.Locks.ContentTree);
                Assert.AreEqual(2, realOuterScope.WriteLocks[outerscope.InstanceId][Constants.Locks.ContentTree]);

                using (var innerScope = scopeProvider.CreateScope())
                {
                    innerscopeId = innerScope.InstanceId;
                    innerScope.WriteLock(Constants.Locks.ContentTree);
                    innerScope.WriteLock(Constants.Locks.ContentTree);
                    Assert.AreEqual(2, realOuterScope.WriteLocks[outerscope.InstanceId][Constants.Locks.ContentTree]);
                    Assert.AreEqual(2, realOuterScope.WriteLocks[innerscopeId][Constants.Locks.ContentTree]);

                    innerScope.WriteLock(Constants.Locks.Languages);
                    innerScope.WriteLock(Constants.Locks.Languages);
                    Assert.AreEqual(2, realOuterScope.WriteLocks[innerScope.InstanceId][Constants.Locks.Languages]);
                    innerScope.Complete();
                }
                Assert.AreEqual(2, realOuterScope.WriteLocks[realOuterScope.InstanceId][Constants.Locks.ContentTree]);
                Assert.IsFalse(realOuterScope.WriteLocks.ContainsKey(innerscopeId));
                outerscope.Complete();
            }
        }

        [Test]
        public void ReadLocks_Count_correctly_If_Lock_Requested_Twice_In_Scope()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            Guid innerscopeId;

            using (var outerscope = scopeProvider.CreateScope())
            {
                var realOuterScope = (Scope) outerscope;
                outerscope.ReadLock(Constants.Locks.ContentTree);
                outerscope.ReadLock(Constants.Locks.ContentTree);
                Assert.AreEqual(2, realOuterScope.ReadLocks[outerscope.InstanceId][Constants.Locks.ContentTree]);

                using (var innerScope = scopeProvider.CreateScope())
                {
                    innerscopeId = innerScope.InstanceId;
                    innerScope.ReadLock(Constants.Locks.ContentTree);
                    innerScope.ReadLock(Constants.Locks.ContentTree);
                    Assert.AreEqual(2, realOuterScope.ReadLocks[outerscope.InstanceId][Constants.Locks.ContentTree]);
                    Assert.AreEqual(2, realOuterScope.ReadLocks[innerScope.InstanceId][Constants.Locks.ContentTree]);

                    innerScope.ReadLock(Constants.Locks.Languages);
                    innerScope.ReadLock(Constants.Locks.Languages);
                    Assert.AreEqual(2, realOuterScope.ReadLocks[innerScope.InstanceId][Constants.Locks.Languages]);
                    innerScope.Complete();
                }
                Assert.AreEqual(2, realOuterScope.ReadLocks[outerscope.InstanceId][Constants.Locks.ContentTree]);
                Assert.IsFalse(realOuterScope.ReadLocks.ContainsKey(innerscopeId));


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
                var realParentScope = (Scope) parentScope;
                parentScope.WriteLock(Constants.Locks.ContentTree);
                parentScope.WriteLock(Constants.Locks.ContentTypes);

                Assert.AreEqual(1, realParentScope.WriteLocks[realParentScope.InstanceId][Constants.Locks.ContentTree], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, realParentScope.WriteLocks[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTypes)}");

                using (var innerScope1 = scopeProvider.CreateScope())
                {
                    innerScope1Id = innerScope1.InstanceId;
                    innerScope1.WriteLock(Constants.Locks.ContentTree);
                    innerScope1.WriteLock(Constants.Locks.ContentTypes);
                    innerScope1.WriteLock(Constants.Locks.Languages);

                    Assert.AreEqual(1, realParentScope.WriteLocks[realParentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope1, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.WriteLocks[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.Languages], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.Languages)}");

                    using (var innerScope2 = scopeProvider.CreateScope())
                    {
                        innerScope2Id = innerScope2.InstanceId;
                        innerScope2.WriteLock(Constants.Locks.ContentTree);
                        innerScope2.WriteLock(Constants.Locks.MediaTypes);

                        Assert.AreEqual(1, realParentScope.WriteLocks[realParentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope2, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, realParentScope.WriteLocks[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope2, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, realParentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, realParentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, realParentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.Languages], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.Languages)}");
                        Assert.AreEqual(1, realParentScope.WriteLocks[innerScope2.InstanceId][Constants.Locks.ContentTree], $"innerScope2, innerScope2 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, realParentScope.WriteLocks[innerScope2.InstanceId][Constants.Locks.MediaTypes], $"innerScope2, innerScope2 instance, after locks acquired: {nameof(Constants.Locks.MediaTypes)}");

                        innerScope2.Complete();
                    }
                    Assert.AreEqual(1, realParentScope.WriteLocks[realParentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope1, parent instance, after innserScope2 disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.WriteLocks[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, parent instance, after innserScope2 disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope1, innerScope1 instance, after innserScope2 disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, innerScope1 instance, after innserScope2 disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.WriteLocks[innerScope1.InstanceId][Constants.Locks.Languages], $"innerScope1, innerScope1 instance, after innserScope2 disposed: {nameof(Constants.Locks.Languages)}");
                    Assert.IsFalse(realParentScope.WriteLocks.ContainsKey(innerScope2Id));

                    innerScope1.Complete();
                }
                Assert.AreEqual(1, realParentScope.WriteLocks[realParentScope.InstanceId][Constants.Locks.ContentTree], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, realParentScope.WriteLocks[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.ContentTypes)}");
                Assert.IsFalse(realParentScope.WriteLocks.ContainsKey(innerScope2Id));
                Assert.IsFalse(realParentScope.WriteLocks.ContainsKey(innerScope1Id));

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
                var realParentScope = (Scope) parentScope;
                parentScope.ReadLock(Constants.Locks.ContentTree);
                parentScope.ReadLock(Constants.Locks.ContentTypes);
                Assert.AreEqual(1, realParentScope.ReadLocks[realParentScope.InstanceId][Constants.Locks.ContentTree], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, realParentScope.ReadLocks[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTypes)}");

                using (var innserScope1 = scopeProvider.CreateScope())
                {
                    innerScope1Id = innserScope1.InstanceId;
                    innserScope1.ReadLock(Constants.Locks.ContentTree);
                    innserScope1.ReadLock(Constants.Locks.ContentTypes);
                    innserScope1.ReadLock(Constants.Locks.Languages);
                    Assert.AreEqual(1, realParentScope.ReadLocks[realParentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope1, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.ReadLocks[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.Languages], $"innerScope1, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.Languages)}");

                    using (var innerScope2 = scopeProvider.CreateScope())
                    {
                        innerScope2Id = innerScope2.InstanceId;
                        innerScope2.ReadLock(Constants.Locks.ContentTree);
                        innerScope2.ReadLock(Constants.Locks.MediaTypes);
                        Assert.AreEqual(1, realParentScope.ReadLocks[realParentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope2, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, realParentScope.ReadLocks[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope2, parent instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, realParentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, realParentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, realParentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.Languages], $"innerScope2, innerScope1 instance, after locks acquired: {nameof(Constants.Locks.Languages)}");
                        Assert.AreEqual(1, realParentScope.ReadLocks[innerScope2.InstanceId][Constants.Locks.ContentTree], $"innerScope2, innerScope2 instance, after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(1, realParentScope.ReadLocks[innerScope2.InstanceId][Constants.Locks.MediaTypes], $"innerScope2, innerScope2 instance, after locks acquired: {nameof(Constants.Locks.MediaTypes)}");

                        innerScope2.Complete();
                    }

                    Assert.AreEqual(1, realParentScope.ReadLocks[realParentScope.InstanceId][Constants.Locks.ContentTree], $"innerScope1, parent instance, after innerScope2 disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.ReadLocks[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, parent instance, after innerScope2 disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.ContentTree], $"innerScope1, innerScope1 instance, after innerScope2 disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(1, realParentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.ContentTypes], $"innerScope1, innerScope1 instance, after innerScope2 disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, realParentScope.ReadLocks[innserScope1.InstanceId][Constants.Locks.Languages], $"innerScope1, innerScope1 instance, after innerScope2 disposed: {nameof(Constants.Locks.Languages)}");
                    Assert.IsFalse(realParentScope.ReadLocks.ContainsKey(innerScope2Id));

                    innserScope1.Complete();
                }

                Assert.AreEqual(1, realParentScope.ReadLocks[realParentScope.InstanceId][Constants.Locks.ContentTree], $"parentScope after innerScope1 disposed: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, realParentScope.ReadLocks[realParentScope.InstanceId][Constants.Locks.ContentTypes], $"parentScope after innerScope1 disposed: {nameof(Constants.Locks.ContentTypes)}");
                Assert.IsFalse(realParentScope.ReadLocks.ContainsKey(innerScope2Id));
                Assert.IsFalse(realParentScope.ReadLocks.ContainsKey(innerScope1Id));

                parentScope.Complete();
            }
        }

        [Test]
        public void WriteLock_Doesnt_Increment_On_Error()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            syntaxProviderMock.Setup(x => x.WriteLock(It.IsAny<IDatabase>(), It.IsAny<int[]>())).Throws(new Exception("Boom"));

            using (var scope = scopeProvider.CreateScope())
            {
                var realScope = (Scope) scope;

                Assert.Throws<Exception>(() => scope.WriteLock(Constants.Locks.Languages));
                Assert.IsFalse(realScope.WriteLocks[scope.InstanceId].ContainsKey(Constants.Locks.Languages));
                scope.Complete();
            }
        }

        [Test]
        public void ReadLock_Doesnt_Increment_On_Error()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);
            syntaxProviderMock.Setup(x => x.ReadLock(It.IsAny<IDatabase>(), It.IsAny<int[]>())).Throws(new Exception("Boom"));

            using (var scope = scopeProvider.CreateScope())
            {
                var realScope = (Scope) scope;

                Assert.Throws<Exception>(() => scope.ReadLock(Constants.Locks.Languages));
                Assert.IsFalse(realScope.ReadLocks[scope.InstanceId].ContainsKey(Constants.Locks.Languages));
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
                scope.ReadLock(Constants.Locks.Domains);

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
                scope.WriteLock(Constants.Locks.Domains);

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

            using (var scope = scopeProvider.CreateScope())
            {
                var realScope = (Scope) scope;
                Assert.IsNull(realScope.WriteLocks);
            }
        }

        [Test]
        public void ReadLocks_Not_Created_Until_First_Lock()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var scope = scopeProvider.CreateScope())
            {
                var realScope = (Scope) scope;
                Assert.IsNull(realScope.ReadLocks);
            }
        }
    }
}
