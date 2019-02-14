using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Security;
using Moq;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;
using Umbraco.Core.Logging;
using Umbraco.Tests.Testing.Objects.Accessors;

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
            // default
            if (!typeof (UmbracoApiControllerBase).IsAssignableFrom(controllerType))
                return base.Create(request, controllerDescriptor, controllerType);

            var owinContext = request.TryGetOwinContext().Result;

            var mockedUserService = Mock.Of<IUserService>();
            var mockedContentService = Mock.Of<IContentService>();
            var mockedMediaService = Mock.Of<IMediaService>();
            var mockedEntityService = Mock.Of<IEntityService>();
            var mockedMemberService = Mock.Of<IMemberService>();
            var mockedMemberTypeService = Mock.Of<IMemberTypeService>();
            var mockedDataTypeService = Mock.Of<IDataTypeService>();
            var mockedContentTypeService = Mock.Of<IContentTypeService>();

            var serviceContext = ServiceContext.CreatePartial(
                userService: mockedUserService,
                contentService: mockedContentService,
                mediaService: mockedMediaService,
                entityService: mockedEntityService,
                memberService: mockedMemberService,
                memberTypeService: mockedMemberTypeService,
                dataTypeService: mockedDataTypeService,
                contentTypeService: mockedContentTypeService,
                localizedTextService:Mock.Of<ILocalizedTextService>());

            var globalSettings = SettingsForTests.GenerateMockGlobalSettings();

            // FIXME: v8?
            ////new app context
            //var dbCtx = new Mock<DatabaseContext>(Mock.Of<IDatabaseFactory>(), Mock.Of<ILogger>(), Mock.Of<ISqlSyntaxProvider>(), "test");
            ////ensure these are set so that the appctx is 'Configured'
            //dbCtx.Setup(x => x.CanConnect).Returns(true);
            //dbCtx.Setup(x => x.IsDatabaseConfigured).Returns(true);
            //var appCtx = ApplicationContext.EnsureContext(
            //    dbCtx.Object,
            //    //pass in mocked services
            //    serviceContext,
            //    CacheHelper.CreateDisabledCacheHelper(),
            //    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()),
            //    true);

            var httpContextItems = new Dictionary<string, object>
            {
                //add the special owin environment to the httpcontext items, this is how the GetOwinContext works
                ["owin.Environment"] = new Dictionary<string, object>()
            };

            //httpcontext with an auth'd user
            var httpContext = Mock.Of<HttpContextBase>(
                http => http.User == owinContext.Authentication.User
                        //ensure the request exists with a cookies collection
                        && http.Request == Mock.Of<HttpRequestBase>(r => r.Cookies == new HttpCookieCollection()
                            && r.RequestContext == new System.Web.Routing.RequestContext
                            {
                                RouteData = new System.Web.Routing.RouteData()
                            })
                        //ensure the request exists with an items collection
                        && http.Items == httpContextItems);
            //chuck it into the props since this is what MS does when hosted and it's needed there
            request.Properties["MS_HttpContext"] = httpContext;

            var backofficeIdentity = (UmbracoBackOfficeIdentity) owinContext.Authentication.User.Identity;

            var webSecurity = new Mock<WebSecurity>(null, null, globalSettings);

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

            var publishedSnapshot = new Mock<IPublishedSnapshot>();
            publishedSnapshot.Setup(x => x.Members).Returns(Mock.Of<IPublishedMemberCache>());
            var publishedSnapshotService = new Mock<IPublishedSnapshotService>();
            publishedSnapshotService.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>())).Returns(publishedSnapshot.Object);

            var umbracoContextAccessor = Umbraco.Web.Composing.Current.UmbracoContextAccessor;

            var umbCtx = UmbracoContext.EnsureContext(
                umbracoContextAccessor,
                httpContext,
                publishedSnapshotService.Object,
                webSecurity.Object,
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == Mock.Of<IWebRoutingSection>(routingSection => routingSection.UrlProviderMode == UrlProviderMode.Auto.ToString())),
                Enumerable.Empty<IUrlProvider>(),
                globalSettings,
                new TestVariationContextAccessor(),
                true); //replace it

            var urlHelper = new Mock<IUrlProvider>();
            urlHelper.Setup(provider => provider.GetUrl(It.IsAny<UmbracoContext>(), It.IsAny<IPublishedContent>(), It.IsAny<UrlProviderMode>(), It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(UrlInfo.Url("/hello/world/1234"));

            var membershipHelper = new MembershipHelper(new TestUmbracoContextAccessor(umbCtx), Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>(), Mock.Of<IMemberService>(), Mock.Of<IMemberTypeService>(), Mock.Of<IUserService>(), Mock.Of<IPublicAccessService>(), Mock.Of<AppCaches>(), Mock.Of<ILogger>());

            var umbHelper = new UmbracoHelper(umbCtx,
                Mock.Of<ITagQuery>(),
                Mock.Of<ICultureDictionaryFactory>(),
                Mock.Of<IUmbracoComponentRenderer>(),
                Mock.Of<IPublishedContentQuery>(),
                membershipHelper);

            return CreateController(controllerType, request, umbracoContextAccessor, umbHelper);
        }

        protected abstract ApiController CreateController(Type controllerType, HttpRequestMessage msg, IUmbracoContextAccessor umbracoContextAccessor, UmbracoHelper helper);
    }
}
