using System;
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
        public void Nested_Scopes_WriteLocks_Count_Correctly()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var outerScope = scopeProvider.CreateScope())
            {
                var parentScope = (Scope) outerScope;
                outerScope.WriteLock(Constants.Locks.ContentTree);
                outerScope.WriteLock(Constants.Locks.ContentTypes);

                Assert.AreEqual(1, parentScope.WriteLocks[Constants.Locks.ContentTree], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, parentScope.WriteLocks[Constants.Locks.ContentTypes], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTypes)}");

                using (var innerScope1 = scopeProvider.CreateScope())
                {
                    innerScope1.WriteLock(Constants.Locks.ContentTree);
                    innerScope1.WriteLock(Constants.Locks.ContentTypes);
                    innerScope1.WriteLock(Constants.Locks.Languages);

                    Assert.AreEqual(2, parentScope.WriteLocks[Constants.Locks.ContentTree], $"childScope1 after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(2, parentScope.WriteLocks[Constants.Locks.ContentTypes], $"childScope1 after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, parentScope.WriteLocks[Constants.Locks.Languages], $"childScope1 after locks acquired: {nameof(Constants.Locks.Languages)}");

                    using (var innerScope2 = scopeProvider.CreateScope())
                    {
                        innerScope2.WriteLock(Constants.Locks.ContentTree);
                        innerScope2.WriteLock(Constants.Locks.MediaTypes);

                        Assert.AreEqual(3, parentScope.WriteLocks[Constants.Locks.ContentTree], $"childScope2 after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(2, parentScope.WriteLocks[Constants.Locks.ContentTypes], $"childScope2 after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, parentScope.WriteLocks[Constants.Locks.Languages], $"childScope2 after locks acquired: {nameof(Constants.Locks.Languages)}");
                        Assert.AreEqual(1, parentScope.WriteLocks[Constants.Locks.MediaTypes], $"childScope2 after locks acquired: {nameof(Constants.Locks.MediaTypes)}");

                        innerScope2.Complete();
                    }
                    Assert.AreEqual(2, parentScope.WriteLocks[Constants.Locks.ContentTree], $"childScope1 after inner scope disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(2, parentScope.WriteLocks[Constants.Locks.ContentTypes], $"childScope1 after inner scope disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, parentScope.WriteLocks[Constants.Locks.Languages], $"childScope1 after inner scope disposed: {nameof(Constants.Locks.Languages)}");
                    Assert.AreEqual(0, parentScope.WriteLocks[Constants.Locks.MediaTypes], $"childScope1 after inner scope disposed: {nameof(Constants.Locks.MediaTypes)}");

                    innerScope1.Complete();
                }
                Assert.AreEqual(1, parentScope.WriteLocks[Constants.Locks.ContentTree], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, parentScope.WriteLocks[Constants.Locks.ContentTypes], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.ContentTypes)}");
                Assert.AreEqual(0, parentScope.WriteLocks[Constants.Locks.Languages], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.Languages)}");
                Assert.AreEqual(0, parentScope.WriteLocks[Constants.Locks.MediaTypes], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.MediaTypes)}");

                outerScope.Complete();
            }
        }

        [Test]
        public void Nested_Scopes_ReadLocks_Count_Correctly()
        {
            var scopeProvider = GetScopeProvider(out var syntaxProviderMock);

            using (var outerScope = scopeProvider.CreateScope())
            {
                var parentScope = (Scope) outerScope;
                outerScope.ReadLock(Constants.Locks.ContentTree);
                outerScope.ReadLock(Constants.Locks.ContentTypes);
                Assert.AreEqual(1, parentScope.ReadLocks[Constants.Locks.ContentTree], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, parentScope.ReadLocks[Constants.Locks.ContentTypes], $"parentScope after locks acquired: {nameof(Constants.Locks.ContentTypes)}");

                using (var innserScope1 = scopeProvider.CreateScope())
                {
                    innserScope1.ReadLock(Constants.Locks.ContentTree);
                    innserScope1.ReadLock(Constants.Locks.ContentTypes);
                    innserScope1.ReadLock(Constants.Locks.Languages);
                    Assert.AreEqual(2, parentScope.ReadLocks[Constants.Locks.ContentTree], $"childScope1 after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(2, parentScope.ReadLocks[Constants.Locks.ContentTypes], $"childScope1 after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, parentScope.ReadLocks[Constants.Locks.Languages], $"childScope1 after locks acquired: {nameof(Constants.Locks.Languages)}");

                    using (var innerScope2 = scopeProvider.CreateScope())
                    {
                        innerScope2.ReadLock(Constants.Locks.ContentTree);
                        innerScope2.ReadLock(Constants.Locks.MediaTypes);
                        Assert.AreEqual(3, parentScope.ReadLocks[Constants.Locks.ContentTree], $"childScope2 after locks acquired: {nameof(Constants.Locks.ContentTree)}");
                        Assert.AreEqual(2, parentScope.ReadLocks[Constants.Locks.ContentTypes], $"childScope2 after locks acquired: {nameof(Constants.Locks.ContentTypes)}");
                        Assert.AreEqual(1, parentScope.ReadLocks[Constants.Locks.Languages], $"childScope2 after locks acquired: {nameof(Constants.Locks.Languages)}");
                        Assert.AreEqual(1, parentScope.ReadLocks[Constants.Locks.MediaTypes], $"childScope2 after locks acquired: {nameof(Constants.Locks.MediaTypes)}");

                        innerScope2.Complete();
                    }
                    Assert.AreEqual(2, parentScope.ReadLocks[Constants.Locks.ContentTree], $"childScope1 after inner scope disposed: {nameof(Constants.Locks.ContentTree)}");
                    Assert.AreEqual(2, parentScope.ReadLocks[Constants.Locks.ContentTypes], $"childScope1 after inner scope disposed: {nameof(Constants.Locks.ContentTypes)}");
                    Assert.AreEqual(1, parentScope.ReadLocks[Constants.Locks.Languages], $"childScope1 after inner scope disposed: {nameof(Constants.Locks.Languages)}");
                    Assert.AreEqual(0, parentScope.ReadLocks[Constants.Locks.MediaTypes], $"childScope1 after inner scope disposed: {nameof(Constants.Locks.MediaTypes)}");

                    innserScope1.Complete();
                }
                Assert.AreEqual(1, parentScope.ReadLocks[Constants.Locks.ContentTree], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.ContentTree)}");
                Assert.AreEqual(1, parentScope.ReadLocks[Constants.Locks.ContentTypes], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.ContentTypes)}");
                Assert.AreEqual(0, parentScope.ReadLocks[Constants.Locks.Languages], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.Languages)}");
                Assert.AreEqual(0, parentScope.ReadLocks[Constants.Locks.MediaTypes], $"parentScope after inner scopes disposed: {nameof(Constants.Locks.MediaTypes)}");

                outerScope.Complete();
            }
        }
    }
}
