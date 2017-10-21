using System.Collections.Generic;
using System.Linq;
using System.Web;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Profiling;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.TokenReplacers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Tests.TokenReplacers
{
    public abstract class TokenReplacerTests
    {
        protected TokenReplacerTests()
        {
            MockContentService = new Mock<IContentService>();
        }

        protected Mock<IContentService> MockContentService { get; set; }

        protected TokenReplacerContext TokenReplacerContext { get; set; }

        [SetUp]
        public virtual void Setup()
        {
            var httpContextFactory = new FakeHttpContextFactory("http://localhost/test");

            var dbCtx = new Mock<DatabaseContext>(Mock.Of<IScopeProviderInternal>(), Mock.Of<ILogger>(), Mock.Of<ISqlSyntaxProvider>(), "test");
            dbCtx.Setup(x => x.IsDatabaseConfigured).Returns(false);

            var appCtx = new ApplicationContext(
                dbCtx.Object,
                new ServiceContext(contentService: MockContentService.Object),
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            var umbCtx = UmbracoContext.CreateContext(
                Mock.Of<HttpContextBase>(),
                appCtx,
                new WebSecurity(Mock.Of<HttpContextBase>(), appCtx),
                Mock.Of<IUmbracoSettingsSection>(), new List<IUrlProvider>(), false);

            TokenReplacerContext = new TokenReplacerContext(httpContextFactory.HttpContext, umbCtx);
        }

        protected ContentItemDisplay GetModel(string propertyValue,
            string contentName = "Test content")
        {
            return new ContentItemDisplay
                {
                    Name = contentName,
                    ParentId = 1000,
                    Tabs = new List<Tab<ContentPropertyDisplay>>
                        {
                            new Tab<ContentPropertyDisplay>()
                                {
                                    Alias = "Tab 1",
                                    Properties = new List<ContentPropertyDisplay>
                                        {
                                            new ContentPropertyDisplay
                                                {
                                                    Alias = "testProperty",
                                                    Value = propertyValue
                                                }
                                        }
                                }
                        }
                };
        }

        protected static string GetTestPropertyValue(ContentItemDisplay model)
        {
            return model.Properties.Single(x => x.Alias == "testProperty").Value.ToString();
        }
    }
}
