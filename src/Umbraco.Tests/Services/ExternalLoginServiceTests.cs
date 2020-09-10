using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.Identity;
using NUnit.Framework;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class ExternalLoginServiceTests : TestWithDatabaseBase
    {
        [Test]
        public void Single_Create()
        {
            var user = new User("Test", "test@test.com", "test", "helloworldtest");
            ServiceContext.UserService.Save(user);

            var extLogin = new IdentityUserLogin("test1", Guid.NewGuid().ToString("N"), user.Id)
            {
                UserData = "hello"
            };
            ServiceContext.ExternalLoginService.Save(extLogin);

            var found = ServiceContext.ExternalLoginService.GetAll(user.Id);

            Assert.AreEqual(1, found.Count());
            Assert.IsTrue(extLogin.HasIdentity);
            Assert.IsTrue(extLogin.Id > 0);
        }

        [Test]
        public void Single_Update()
        {
            var user = new User("Test", "test@test.com", "test", "helloworldtest");
            ServiceContext.UserService.Save(user);

            var extLogin = new IdentityUserLogin("test1", Guid.NewGuid().ToString("N"), user.Id)
            {
                UserData = "hello"
            };
            ServiceContext.ExternalLoginService.Save(extLogin);

            extLogin.UserData = "world";
            ServiceContext.ExternalLoginService.Save(extLogin);

            var found = ServiceContext.ExternalLoginService.GetAll(user.Id).Cast<IIdentityUserLoginExtended>().ToList();
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual("world", found[0].UserData);
        }

        [Test]
        public void Add_Logins()
        {
            var user = new User("Test", "test@test.com", "test", "helloworldtest");
            ServiceContext.UserService.Save(user);

            var externalLogins = new[]
            {
                new ExternalLogin("test1", Guid.NewGuid().ToString("N")),
                new ExternalLogin("test2", Guid.NewGuid().ToString("N"))
            };

            ServiceContext.ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ServiceContext.ExternalLoginService.GetAll(user.Id).OrderBy(x => x.LoginProvider).ToList();
            Assert.AreEqual(2, logins.Count);
            for (int i = 0; i < logins.Count; i++)
            {
                Assert.AreEqual(logins[i].ProviderKey, externalLogins[i].ProviderKey);
                Assert.AreEqual(logins[i].LoginProvider, externalLogins[i].LoginProvider);
            }
        }

        [Test]
        public void Add_Update_Delete_Logins()
        {
            var user = new User("Test", "test@test.com", "test", "helloworldtest");
            ServiceContext.UserService.Save(user);

            var externalLogins = new[]
            {
                new ExternalLogin("test1", Guid.NewGuid().ToString("N")),
                new ExternalLogin("test2", Guid.NewGuid().ToString("N")),
                new ExternalLogin("test3", Guid.NewGuid().ToString("N")),
                new ExternalLogin("test4", Guid.NewGuid().ToString("N"))
            };

            ServiceContext.ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ServiceContext.ExternalLoginService.GetAll(user.Id).OrderBy(x => x.LoginProvider).ToList();

            logins.RemoveAt(0); // remove the first one
            logins.Add(new IdentityUserLogin("test5", Guid.NewGuid().ToString("N"), user.Id)); // add a new one

            // save new list
            ServiceContext.ExternalLoginService.Save(user.Id, logins.Select(x => new ExternalLogin(x.LoginProvider, x.ProviderKey)));

            var updatedLogins = ServiceContext.ExternalLoginService.GetAll(user.Id).OrderBy(x => x.LoginProvider).ToList();
            Assert.AreEqual(4, updatedLogins.Count);
            for (int i = 0; i < updatedLogins.Count; i++)
            {                
                Assert.AreEqual(logins[i].LoginProvider, updatedLogins[i].LoginProvider);
            }
        }

        [Test]
        public void Add_Retrieve_User_Data()
        {
            var user = new User("Test", "test@test.com", "test", "helloworldtest");
            ServiceContext.UserService.Save(user);

            var externalLogins = new[]
            {
                new ExternalLogin("test1", Guid.NewGuid().ToString("N"), "hello world")
            };

            ServiceContext.ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ServiceContext.ExternalLoginService.GetAll(user.Id).Cast<IIdentityUserLoginExtended>().ToList();

            Assert.AreEqual("hello world", logins[0].UserData);

        }
    }
}
