using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping
{
    /// <summary>
    /// These tests verify that the various types of scopes we have can be created and disposed within each other.
    /// </summary>
    /// <remarks>
    /// Scopes are:
    ///  - "Normal" - created by <see cref="IScopeProvider"/>"/>.
    ///  - "Core" - created by <see cref="ICoreScopeProvider"/>"/>.
    ///  - "EFCore" - created by <see cref="IEFCoreScopeProvider{TDbContext}"/>"/>.
    /// </remarks>
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    internal sealed class NestedScopeTests : UmbracoIntegrationTest
    {
        private new IScopeProvider ScopeProvider => Services.GetRequiredService<IScopeProvider>();

        private ICoreScopeProvider CoreScopeProvider => Services.GetRequiredService<ICoreScopeProvider>();

        private IEFCoreScopeProvider<TestUmbracoDbContext> EFCoreScopeProvider =>
            Services.GetRequiredService<IEFCoreScopeProvider<TestUmbracoDbContext>>();

        [Test]
        public void CanNestScopes_Normal_Core_EFCore()
        {
            using (var ambientScope = ScopeProvider.CreateScope())
            {
                ambientScope.WriteLock(Constants.Locks.ContentTree);

                using (var outerScope = CoreScopeProvider.CreateCoreScope())
                {
                    outerScope.WriteLock(Constants.Locks.ContentTree);

                    using (var innerScope = EFCoreScopeProvider.CreateScope())
                    {
                        innerScope.WriteLock(Constants.Locks.ContentTree);

                        innerScope.Complete();
                        outerScope.Complete();
                        ambientScope.Complete();
                    }
                }
            }
        }

        [Test]
        public void CanNestScopes_Normal_EFCore_Core()
        {
            using (var ambientScope = ScopeProvider.CreateScope())
            {
                ambientScope.WriteLock(Constants.Locks.ContentTree);

                using (var outerScope = EFCoreScopeProvider.CreateScope())
                {
                    outerScope.WriteLock(Constants.Locks.ContentTree);

                    using (var innerScope = CoreScopeProvider.CreateCoreScope())
                    {
                        innerScope.WriteLock(Constants.Locks.ContentTree);

                        innerScope.Complete();
                        outerScope.Complete();
                        ambientScope.Complete();
                    }
                }
            }
        }

        [Test]
        public void CanNestScopes_Core_Normal_EFCore()
        {
            using (var ambientScope = CoreScopeProvider.CreateCoreScope())
            {
                ambientScope.WriteLock(Constants.Locks.ContentTree);

                using (var outerScope = ScopeProvider.CreateScope())
                {
                    outerScope.WriteLock(Constants.Locks.ContentTree);

                    using (var innerScope = EFCoreScopeProvider.CreateScope())
                    {
                        innerScope.WriteLock(Constants.Locks.ContentTree);

                        innerScope.Complete();
                        outerScope.Complete();
                        ambientScope.Complete();
                    }
                }
            }
        }

        [Test]
        public void CanNestScopes_Core_EFCore_Normal()
        {
            using (var ambientScope = CoreScopeProvider.CreateCoreScope())
            {
                ambientScope.WriteLock(Constants.Locks.ContentTree);

                using (var outerScope = EFCoreScopeProvider.CreateScope())
                {
                    outerScope.WriteLock(Constants.Locks.ContentTree);

                    using (var innerScope = ScopeProvider.CreateScope())
                    {
                        innerScope.WriteLock(Constants.Locks.ContentTree);

                        innerScope.Complete();
                        outerScope.Complete();
                        ambientScope.Complete();
                    }
                }
            }
        }

        [Test]
        public void CanNestScopes_EFCore_Normal_Core()
        {
            using (var ambientScope = EFCoreScopeProvider.CreateScope())
            {
                ambientScope.WriteLock(Constants.Locks.ContentTree);

                using (var outerScope = ScopeProvider.CreateScope())
                {
                    outerScope.WriteLock(Constants.Locks.ContentTree);

                    using (var innerScope = CoreScopeProvider.CreateCoreScope())
                    {
                        innerScope.WriteLock(Constants.Locks.ContentTree);

                        innerScope.Complete();
                        outerScope.Complete();
                        ambientScope.Complete();
                    }
                }
            }
        }

        [Test]
        public void CanNestScopes_EFCore_Core_Normal()
        {
            using (var ambientScope = EFCoreScopeProvider.CreateScope())
            {
                ambientScope.WriteLock(Constants.Locks.ContentTree);

                using (var outerScope = CoreScopeProvider.CreateCoreScope())
                {
                    outerScope.WriteLock(Constants.Locks.ContentTree);

                    using (var innerScope = ScopeProvider.CreateScope())
                    {
                        innerScope.WriteLock(Constants.Locks.ContentTree);

                        innerScope.Complete();
                        outerScope.Complete();
                        ambientScope.Complete();
                    }
                }
            }
        }

        [Test]
        public void CanNestScopes_Normal_Normal()
        {
            using (var ambientScope = ScopeProvider.CreateScope())
            {
                ambientScope.WriteLock(Constants.Locks.ContentTree);

                using (var inner = ScopeProvider.CreateScope())
                {
                    inner.WriteLock(Constants.Locks.ContentTree);

                    inner.Complete();
                    ambientScope.Complete();
                }
            }
        }

        [Test]
        public void CanNestScopes_Core_Core()
        {
            using (var ambientScope = CoreScopeProvider.CreateCoreScope())
            {
                ambientScope.WriteLock(Constants.Locks.ContentTree);

                using (var inner = CoreScopeProvider.CreateCoreScope())
                {
                    inner.WriteLock(Constants.Locks.ContentTree);

                    inner.Complete();
                    ambientScope.Complete();
                }
            }
        }

        [Test]
        public void CanNestScopes_EFCore_EFCore()
        {
            using (var ambientScope = EFCoreScopeProvider.CreateScope())
            {
                ambientScope.WriteLock(Constants.Locks.ContentTree);

                using (var inner = EFCoreScopeProvider.CreateScope())
                {
                    inner.WriteLock(Constants.Locks.ContentTree);

                    inner.Complete();
                    ambientScope.Complete();
                }
            }
        }
    }
}
