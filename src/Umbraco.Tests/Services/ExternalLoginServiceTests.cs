using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.Identity;
using NUnit.Framework;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Dtos;
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
        public void Removes_Existing_Duplicates_On_Save()
        {
            var user = new User("Test", "test@test.com", "test", "helloworldtest");
            ServiceContext.UserService.Save(user);

            var providerKey = Guid.NewGuid().ToString("N");
            var latest = DateTime.Now.AddDays(-1);
            var oldest = DateTime.Now.AddDays(-10);

            using (var scope = ScopeProvider.CreateScope())
            {
                // insert duplicates manuall
                scope.Database.Insert(new ExternalLoginDto
                {
                    UserId = user.Id,
                    LoginProvider = "test1",
                    ProviderKey = providerKey,
                    CreateDate = latest
                });
                scope.Database.Insert(new ExternalLoginDto
                {
                    UserId = user.Id,
                    LoginProvider = "test1",
                    ProviderKey = providerKey,
                    CreateDate = oldest
                });
            }

            // try to save 2 other duplicates
            var externalLogins = new[]
            {
                new ExternalLogin("test2", providerKey),
                new ExternalLogin("test2", providerKey),
                new ExternalLogin("test1", providerKey)
            };

            ServiceContext.ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ServiceContext.ExternalLoginService.GetAll(user.Id).ToList();

            // duplicates will be removed, keeping the latest entries
            Assert.AreEqual(2, logins.Count);

            var test1 = logins.Single(x => x.LoginProvider == "test1");
            Assert.Greater(test1.CreateDate, latest);
        }

        [Test]
        public void Does_Not_Persist_Duplicates()
        {
            var user = new User("Test", "test@test.com", "test", "helloworldtest");
            ServiceContext.UserService.Save(user);

            var providerKey = Guid.NewGuid().ToString("N");
            var externalLogins = new[]
            {
                new ExternalLogin("test1", providerKey),
                new ExternalLogin("test1", providerKey)
            };

            ServiceContext.ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ServiceContext.ExternalLoginService.GetAll(user.Id).ToList();
            Assert.AreEqual(1, logins.Count);
        }

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
        public void Multiple_Update()
        {
            var user = new User("Test", "test@test.com", "test", "helloworldtest");
            ServiceContext.UserService.Save(user);

            var providerKey1 = Guid.NewGuid().ToString("N");
            var providerKey2 = Guid.NewGuid().ToString("N");
            var extLogins = new[]
            {
                new ExternalLogin("test1", providerKey1, "hello"),
                new ExternalLogin("test2", providerKey2, "world")
            };
            ServiceContext.ExternalLoginService.Save(user.Id, extLogins);

            extLogins = new[]
            {
                new ExternalLogin("test1", providerKey1, "123456"),
                new ExternalLogin("test2", providerKey2, "987654")
            };
            ServiceContext.ExternalLoginService.Save(user.Id, extLogins);

            var found = ServiceContext.ExternalLoginService.GetAll(user.Id).Cast<IIdentityUserLoginExtended>().OrderBy(x => x.LoginProvider).ToList();
            Assert.AreEqual(2, found.Count);
            Assert.AreEqual("123456", found[0].UserData);
            Assert.AreEqual("987654", found[1].UserData);
        }

        [Test]
        public void Can_Find_As_Extended_Type()
        {
            var user = new User("Test", "test@test.com", "test", "helloworldtest");
            ServiceContext.UserService.Save(user);

            var providerKey1 = Guid.NewGuid().ToString("N");
            var providerKey2 = Guid.NewGuid().ToString("N");
            var extLogins = new[]
            {
                new ExternalLogin("test1", providerKey1, "hello"),
                new ExternalLogin("test2", providerKey2, "world")
            };
            ServiceContext.ExternalLoginService.Save(user.Id, extLogins);

            var found = ServiceContext.ExternalLoginService.Find("test2", providerKey2).ToList();
            Assert.AreEqual(1, found.Count);
            var asExtended = found.Cast<IIdentityUserLoginExtended>().ToList();
            Assert.AreEqual(1, found.Count);

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
