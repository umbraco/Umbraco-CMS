using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.SelfHost;
using AutoMapper;
using Examine.Providers;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.ControllerTesting;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.WebApi;

namespace Umbraco.Tests.Controllers
{
    [DatabaseTestBehavior(DatabaseBehavior.NoDatabasePerFixture)]
    [RequiresAutoMapperMappings]
    [TestFixture]
    public class UsersControllerTests : BaseDatabaseFactoryTest
    {
        [Test]
        public async void GetPagedUsers_Empty()
        {
            var runner = new TestRunner((message, helper) => new UsersController(helper.UmbracoContext));
            var response = await runner.Execute("Users", "GetPagedUsers", HttpMethod.Get);

            var obj = JsonConvert.DeserializeObject<PagedResult<UserDisplay>>(response.Item2);
            Assert.AreEqual(0, obj.TotalItems);
        }

        [Test]
        public async void GetPagedUsers_10()
        {
            var runner = new TestRunner((message, helper) =>
            {
                //setup some mocks
                var userServiceMock = Mock.Get(helper.UmbracoContext.Application.Services.UserService);
                var users = MockedUser.CreateMulipleUsers(10);
                long outVal = 10;
                userServiceMock.Setup(service => service.GetAll(It.IsAny<long>(), It.IsAny<int>(), out outVal, It.IsAny<string>(), It.IsAny<Direction>(), It.IsAny<UserState?>(), It.IsAny<string[]>(), It.IsAny<string>()))
                    .Returns(() => users);

                return new UsersController(helper.UmbracoContext);
            });
            var response = await runner.Execute("Users", "GetPagedUsers", HttpMethod.Get);

            var obj = JsonConvert.DeserializeObject<PagedResult<UserDisplay>>(response.Item2);
            Assert.AreEqual(10, obj.TotalItems);
            Assert.AreEqual(10, obj.Items.Count());
        }
    }
}
