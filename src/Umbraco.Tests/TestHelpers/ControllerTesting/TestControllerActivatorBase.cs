using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Security;
using Moq;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;

namespace Umbraco.Tests.TestHelpers.ControllerTesting
{
    /// <summary>
    /// Used to mock all of the services required for re-mocking and testing controllers
    /// </summary>
    /// <remarks>
    /// A more complete version of this is found in the Umbraco REST API project but this has the basics covered
    /// </remarks>
    public abstract class TestControllerActivatorBase : DefaultHttpControllerActivator, IHttpControllerActivator
    {
        IHttpController IHttpControllerActivator.Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            if (typeof(UmbracoApiControllerBase).IsAssignableFrom(controllerType))
            {
                var owinContext = request.GetOwinContext();
                
                var mockedUserService = Mock.Of<IUserService>();
                var mockedContentService = Mock.Of<IContentService>();
                var mockedMediaService = Mock.Of<IMediaService>();
                var mockedEntityService = Mock.Of<IEntityService>();
                var mockedMemberService = Mock.Of<IMemberService>();
                var mockedMemberTypeService = Mock.Of<IMemberTypeService>();
                var mockedDataTypeService = Mock.Of<IDataTypeService>();
                var mockedContentTypeService = Mock.Of<IContentTypeService>();

                var mockedMigrationService = new Mock<IMigrationEntryService>();
                //set it up to return anything so that the app ctx is 'Configured'
                mockedMigrationService.Setup(x => x.FindEntry(It.IsAny<string>(), It.IsAny<SemVersion>())).Returns(Mock.Of<IMigrationEntry>());

                var serviceContext = new ServiceContext(
                    userService: mockedUserService,
                    contentService: mockedContentService,
                    mediaService: mockedMediaService,
                    entityService: mockedEntityService,
                    memberService: mockedMemberService,
                    memberTypeService: mockedMemberTypeService,
                    dataTypeService: mockedDataTypeService,
                    contentTypeService: mockedContentTypeService,
                    migrationEntryService: mockedMigrationService.Object,
                    localizedTextService:Mock.Of<ILocalizedTextService>(),
                    sectionService:Mock.Of<ISectionService>());

                //ensure the configuration matches the current version for tests
                SettingsForTests.ConfigurationStatus = UmbracoVersion.GetSemanticVersion().ToSemanticString();

                //new app context
                var dbCtx = new Mock<DatabaseContext>(Mock.Of<IDatabaseFactory>(), Mock.Of<ILogger>(), Mock.Of<ISqlSyntaxProvider>(), "test");
                //ensure these are set so that the appctx is 'Configured'
                dbCtx.Setup(x => x.CanConnect).Returns(true);
                dbCtx.Setup(x => x.IsDatabaseConfigured).Returns(true);
                var appCtx = ApplicationContext.EnsureContext(
                    dbCtx.Object,
                    //pass in mocked services
                    serviceContext,
                    CacheHelper.CreateDisabledCacheHelper(),
                    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()),
                    true);

                //httpcontext with an auth'd user
                var httpContext = Mock.Of<HttpContextBase>(
                    http => http.User == owinContext.Authentication.User
                            //ensure the request exists with a cookies collection    
                            && http.Request == Mock.Of<HttpRequestBase>(r => r.Cookies == new HttpCookieCollection())
                            //ensure the request exists with an items collection    
                            && http.Items == Mock.Of<IDictionary>());
                //chuck it into the props since this is what MS does when hosted and it's needed there
                request.Properties["MS_HttpContext"] = httpContext;                

                var backofficeIdentity = (UmbracoBackOfficeIdentity)owinContext.Authentication.User.Identity;

                var webSecurity = new Mock<WebSecurity>(null, null);

                //mock CurrentUser
                var groups = new List<ReadOnlyUserGroup>();
                for (var index = 0; index < backofficeIdentity.Roles.Length; index++)
                {
                    var role = backofficeIdentity.Roles[index];
                    groups.Add(new ReadOnlyUserGroup(index + 1, role, "icon-user", null, null, role, new string[0], new string[0]));
                }
                webSecurity.Setup(x => x.CurrentUser)
                    .Returns(Mock.Of<IUser>(u => u.IsApproved == true
                                                 && u.IsLockedOut == false
                                                 && u.AllowedSections == backofficeIdentity.AllowedApplications
                                                 && u.Groups == groups
                                                 && u.Email == "admin@admin.com"
                                                 && u.Id == (int) backofficeIdentity.Id
                                                 && u.Language == "en"
                                                 && u.Name == backofficeIdentity.RealName
                                                 && u.StartContentIds == backofficeIdentity.StartContentNodes
                                                 && u.StartMediaIds == backofficeIdentity.StartMediaNodes
                                                 && u.Username == backofficeIdentity.Username));

                //mock Validate
                webSecurity.Setup(x => x.ValidateCurrentUser())
                    .Returns(() => true);               
                webSecurity.Setup(x => x.UserHasSectionAccess(It.IsAny<string>(), It.IsAny<IUser>()))
                    .Returns(() => true);

                var umbCtx = UmbracoContext.EnsureContext(
                    //set the user of the HttpContext
                    httpContext,
                    appCtx,
                    webSecurity.Object,
                    Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == Mock.Of<IWebRoutingSection>(routingSection => routingSection.UrlProviderMode == UrlProviderMode.Auto.ToString())),
                    Enumerable.Empty<IUrlProvider>(),
                    true); //replace it

                var urlHelper = new Mock<IUrlProvider>();
                urlHelper.Setup(provider => provider.GetUrl(It.IsAny<UmbracoContext>(), It.IsAny<int>(), It.IsAny<Uri>(), It.IsAny<UrlProviderMode>()))
                    .Returns("/hello/world/1234");

                var membershipHelper = new MembershipHelper(umbCtx, Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>());

                var mockedTypedContent = Mock.Of<ITypedPublishedContentQuery>();

                var umbHelper = new UmbracoHelper(umbCtx,
                    Mock.Of<IPublishedContent>(),
                    mockedTypedContent,
                    Mock.Of<IDynamicPublishedContentQuery>(),
                    Mock.Of<ITagQuery>(),
                    Mock.Of<IDataTypeService>(),
                    new UrlProvider(umbCtx, new[]
                    {
                        urlHelper.Object
                    }, UrlProviderMode.Auto),
                    Mock.Of<ICultureDictionary>(),
                    Mock.Of<IUmbracoComponentRenderer>(),
                    membershipHelper);

                return CreateController(controllerType, request, umbHelper);
            }
            //default
            return base.Create(request, controllerDescriptor, controllerType);
        }

        protected abstract ApiController CreateController(Type controllerType, HttpRequestMessage msg, UmbracoHelper helper);
    }
}
