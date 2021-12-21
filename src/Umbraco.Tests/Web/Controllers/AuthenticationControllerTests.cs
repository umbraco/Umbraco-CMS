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
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.ControllerTesting;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.Features;
using Umbraco.Web.Models.ContentEditing;
using IUser = Umbraco.Core.Models.Membership.IUser;

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
            Composition.RegisterUnique(f => Mock.Of<IUserService>());

            // kill the true IEntityService too
            Composition.RegisterUnique(f => Mock.Of<IEntityService>());

            Composition.RegisterUnique<UmbracoFeatures>();
        }


        [Test]
        public async System.Threading.Tasks.Task GetCurrentUser_Fips()
        {
            ApiController CtrlFactory(HttpRequestMessage message, IUmbracoContextAccessor umbracoContextAccessor, UmbracoHelper helper)
            {
                //setup some mocks
                var userServiceMock = Mock.Get(Current.Services.UserService);
                userServiceMock.Setup(service => service.GetUserById(It.IsAny<int>()))
                    .Returns(() => null);

                if (Thread.GetDomain().GetData(".appPath") != null)
                {
                    HttpContext.Current = new HttpContext(new SimpleWorkerRequest("", "", new StringWriter()));
                }
                else
                {
                    var baseDir = IOHelper.MapPath("", false).TrimEnd(IOHelper.DirSepChar);
                    HttpContext.Current = new HttpContext(new SimpleWorkerRequest("/", baseDir, "", "", new StringWriter()));
                }
                IOHelper.ForceNotHosted = true;
                var usersController = new AuthenticationController(
                    Factory.GetInstance<IGlobalSettings>(),
                    umbracoContextAccessor,
                    Factory.GetInstance<ISqlContext>(),
                    Factory.GetInstance<ServiceContext>(),
                    Factory.GetInstance<AppCaches>(),
                    Factory.GetInstance<IProfilingLogger>(),
                    Factory.GetInstance<IRuntimeState>(),
                    helper,
                    Factory.GetInstance<IUmbracoSettingsSection>());
                return usersController;
            }

            Mock.Get(Current.SqlContext)
                .Setup(x => x.Query<IUser>())
                .Returns(new Query<IUser>(Current.SqlContext));

            var syntax = new SqlCeSyntaxProvider();

            Mock.Get(Current.SqlContext)
                .Setup(x => x.SqlSyntax)
                .Returns(syntax);

            var mappers = new MapperCollection(new[]
            {
                new UserMapper(new Lazy<ISqlContext>(() => Current.SqlContext), new ConcurrentDictionary<Type, ConcurrentDictionary<string, string>>())
            });

            Mock.Get(Current.SqlContext)
                .Setup(x => x.Mappers)
                .Returns(mappers);

            // Testing what happens if the system were configured to only use FIPS-compliant algorithms
            var typ = typeof(CryptoConfig);
            var flds = typ.GetFields(BindingFlags.Static | BindingFlags.NonPublic);
            var haveFld = flds.FirstOrDefault(f => f.Name == "s_haveFipsAlgorithmPolicy");
            var isFld = flds.FirstOrDefault(f => f.Name == "s_fipsAlgorithmPolicy");
            var originalFipsValue = CryptoConfig.AllowOnlyFipsAlgorithms;

            try
            {
                if (!originalFipsValue)
                {
                    haveFld.SetValue(null, true);
                    isFld.SetValue(null, true);
                }

                var runner = new TestRunner(CtrlFactory);
                var response = await runner.Execute("Authentication", "GetCurrentUser", HttpMethod.Get);

                var obj = JsonConvert.DeserializeObject<UserDetail>(response.Item2);
                Assert.AreEqual(-1, obj.UserId);
            }
            finally
            {
                if (!originalFipsValue)
                {
                    haveFld.SetValue(null, false);
                    isFld.SetValue(null, false);
                }
            }
        }
    }
}
