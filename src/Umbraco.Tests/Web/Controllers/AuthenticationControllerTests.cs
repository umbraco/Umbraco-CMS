using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Web.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.ControllerTesting;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.Features;
using Umbraco.Web.Models.ContentEditing;
using IUser = Umbraco.Core.Models.Membership.IUser;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Persistance.SqlCe;
using Umbraco.Web.Routing;
using Umbraco.Core.DependencyInjection;

namespace Umbraco.Tests.Web.Controllers
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.None)]
    public class AuthenticationControllerTests : TestWithDatabaseBase
    {
        protected override void ComposeApplication(bool withApplication)
        {
            base.ComposeApplication(withApplication);
            //if (!withApplication) return;

            // replace the true IUserService implementation with a mock
            // so that each test can configure the service to their liking
            Builder.Services.AddUnique(f => Mock.Of<IUserService>());

            // kill the true IEntityService too
            Builder.Services.AddUnique(f => Mock.Of<IEntityService>());

            Builder.Services.AddUnique<UmbracoFeatures>();
        }


        // TODO Reintroduce when moved to .NET Core
        // [Test]
        // public async System.Threading.Tasks.Task GetCurrentUser_Fips()
        // {
        //     ApiController CtrlFactory(HttpRequestMessage message, IUmbracoContextAccessor umbracoContextAccessor)
        //     {
        //         //setup some mocks
        //         var userServiceMock = Mock.Get(ServiceContext.UserService);
        //         userServiceMock.Setup(service => service.GetUserById(It.IsAny<int>()))
        //             .Returns(() => null);
        //
        //         if (Thread.GetDomain().GetData(".appPath") != null)
        //         {
        //             HttpContext.Current = new HttpContext(new SimpleWorkerRequest("", "", new StringWriter()));
        //         }
        //         else
        //         {
        //             var baseDir = IOHelper.MapPath("").TrimEnd(Path.DirectorySeparatorChar);
        //             HttpContext.Current = new HttpContext(new SimpleWorkerRequest("/", baseDir, "", "", new StringWriter()));
        //         }
        //
        //         var usersController = new AuthenticationController(
        //             new TestUserPasswordConfig(),
        //             Factory.GetInstance<IGlobalSettings>(),
        //             Factory.GetInstance<IHostingEnvironment>(),
        //             umbracoContextAccessor,
        //             Factory.GetInstance<ISqlContext>(),
        //             Factory.GetInstance<ServiceContext>(),
        //             Factory.GetInstance<AppCaches>(),
        //             Factory.GetInstance<IProfilingLogger>(),
        //             Factory.GetInstance<IRuntimeState>(),
        //             Factory.GetInstance<UmbracoMapper>(),
        //             Factory.GetInstance<ISecuritySettings>(),
        //             Factory.GetInstance<IPublishedUrlProvider>(),
        //             Factory.GetInstance<IRequestAccessor>(),
        //             Factory.GetInstance<IEmailSender>()
        //         );
        //         return usersController;
        //     }
        //
        //     Mock.Get(Current.SqlContext)
        //         .Setup(x => x.Query<IUser>())
        //         .Returns(new Query<IUser>(Current.SqlContext));
        //
        //     var syntax = new SqlCeSyntaxProvider();
        //
        //     Mock.Get(Current.SqlContext)
        //         .Setup(x => x.SqlSyntax)
        //         .Returns(syntax);
        //
        //     var mappers = new MapperCollection(new[]
        //     {
        //         new UserMapper(new Lazy<ISqlContext>(() => Current.SqlContext), new ConcurrentDictionary<Type, ConcurrentDictionary<string, string>>())
        //     });
        //
        //     Mock.Get(Current.SqlContext)
        //         .Setup(x => x.Mappers)
        //         .Returns(mappers);
        //
        //     // Testing what happens if the system were configured to only use FIPS-compliant algorithms
        //     var typ = typeof(CryptoConfig);
        //     var flds = typ.GetFields(BindingFlags.Static | BindingFlags.NonPublic);
        //     var haveFld = flds.FirstOrDefault(f => f.Name == "s_haveFipsAlgorithmPolicy");
        //     var isFld = flds.FirstOrDefault(f => f.Name == "s_fipsAlgorithmPolicy");
        //     var originalFipsValue = CryptoConfig.AllowOnlyFipsAlgorithms;
        //
        //     try
        //     {
        //         if (!originalFipsValue)
        //         {
        //             haveFld.SetValue(null, true);
        //             isFld.SetValue(null, true);
        //         }
        //
        //         var runner = new TestRunner(CtrlFactory);
        //         var response = await runner.Execute("Authentication", "GetCurrentUser", HttpMethod.Get);
        //
        //         var obj = JsonConvert.DeserializeObject<UserDetail>(response.Item2);
        //         Assert.AreEqual(-1, obj.UserId);
        //     }
        //     finally
        //     {
        //         if (!originalFipsValue)
        //         {
        //             haveFld.SetValue(null, false);
        //             isFld.SetValue(null, false);
        //         }
        //     }
        // }
    }
}
