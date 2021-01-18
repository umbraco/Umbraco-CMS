// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Services;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class ExternalLoginServiceTests : UmbracoIntegrationTest
    {
        private IUserService UserService => GetRequiredService<IUserService>();

        private IExternalLoginService ExternalLoginService => GetRequiredService<IExternalLoginService>();

        [Test]
        public void Removes_Existing_Duplicates_On_Save()
        {
            var user = new User(GlobalSettings, "Test", "test@test.com", "test", "helloworldtest");
            UserService.Save(user);

            string providerKey = Guid.NewGuid().ToString("N");
            DateTime latest = DateTime.Now.AddDays(-1);
            DateTime oldest = DateTime.Now.AddDays(-10);

            using (Core.Scoping.IScope scope = ScopeProvider.CreateScope())
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
            ExternalLogin[] externalLogins = new[]
            {
                new ExternalLogin("test2", providerKey),
                new ExternalLogin("test2", providerKey),
                new ExternalLogin("test1", providerKey)
            };

            ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ExternalLoginService.GetAll(user.Id).ToList();

            // duplicates will be removed, keeping the latest entries
            Assert.AreEqual(2, logins.Count);

            IIdentityUserLogin test1 = logins.Single(x => x.LoginProvider == "test1");
            Assert.Greater(test1.CreateDate, latest);
        }

        [Test]
        public void Does_Not_Persist_Duplicates()
        {
            var user = new User(GlobalSettings, "Test", "test@test.com", "test", "helloworldtest");
            UserService.Save(user);

            string providerKey = Guid.NewGuid().ToString("N");
            ExternalLogin[] externalLogins = new[]
            {
                new ExternalLogin("test1", providerKey),
                new ExternalLogin("test1", providerKey)
            };

            ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ExternalLoginService.GetAll(user.Id).ToList();
            Assert.AreEqual(1, logins.Count);
        }

        [Test]
        public void Single_Create()
        {
            var user = new User(GlobalSettings, "Test", "test@test.com", "test", "helloworldtest");
            UserService.Save(user);

            var extLogin = new IdentityUserLogin("test1", Guid.NewGuid().ToString("N"), user.Id.ToString())
            {
                UserData = "hello"
            };
            ExternalLoginService.Save(extLogin);

            IEnumerable<IIdentityUserLogin> found = ExternalLoginService.GetAll(user.Id);

            Assert.AreEqual(1, found.Count());
            Assert.IsTrue(extLogin.HasIdentity);
            Assert.IsTrue(extLogin.Id > 0);
        }

        [Test]
        public void Single_Update()
        {
            var user = new User(GlobalSettings, "Test", "test@test.com", "test", "helloworldtest");
            UserService.Save(user);

            var extLogin = new IdentityUserLogin("test1", Guid.NewGuid().ToString("N"), user.Id.ToString())
            {
                UserData = "hello"
            };
            ExternalLoginService.Save(extLogin);

            extLogin.UserData = "world";
            ExternalLoginService.Save(extLogin);

            var found = ExternalLoginService.GetAll(user.Id).ToList();
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual("world", found[0].UserData);
        }

        [Test]
        public void Multiple_Update()
        {
            var user = new User(GlobalSettings, "Test", "test@test.com", "test", "helloworldtest");
            UserService.Save(user);

            string providerKey1 = Guid.NewGuid().ToString("N");
            string providerKey2 = Guid.NewGuid().ToString("N");
            ExternalLogin[] extLogins = new[]
            {
                new ExternalLogin("test1", providerKey1, "hello"),
                new ExternalLogin("test2", providerKey2, "world")
            };
            ExternalLoginService.Save(user.Id, extLogins);

            extLogins = new[]
            {
                new ExternalLogin("test1", providerKey1, "123456"),
                new ExternalLogin("test2", providerKey2, "987654")
            };
            ExternalLoginService.Save(user.Id, extLogins);

            var found = ExternalLoginService.GetAll(user.Id).OrderBy(x => x.LoginProvider).ToList();
            Assert.AreEqual(2, found.Count);
            Assert.AreEqual("123456", found[0].UserData);
            Assert.AreEqual("987654", found[1].UserData);
        }

        [Test]
        public void Can_Find_As_Extended_Type()
        {
            var user = new User(GlobalSettings, "Test", "test@test.com", "test", "helloworldtest");
            UserService.Save(user);

            string providerKey1 = Guid.NewGuid().ToString("N");
            string providerKey2 = Guid.NewGuid().ToString("N");
            ExternalLogin[] extLogins = new[]
            {
                new ExternalLogin("test1", providerKey1, "hello"),
                new ExternalLogin("test2", providerKey2, "world")
            };
            ExternalLoginService.Save(user.Id, extLogins);

            var found = ExternalLoginService.Find("test2", providerKey2).ToList();
            Assert.AreEqual(1, found.Count);
            var asExtended = found.ToList();
            Assert.AreEqual(1, found.Count);
        }

        [Test]
        public void Add_Logins()
        {
            var user = new User(GlobalSettings, "Test", "test@test.com", "test", "helloworldtest");
            UserService.Save(user);

            ExternalLogin[] externalLogins = new[]
            {
                new ExternalLogin("test1", Guid.NewGuid().ToString("N")),
                new ExternalLogin("test2", Guid.NewGuid().ToString("N"))
            };

            ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ExternalLoginService.GetAll(user.Id).OrderBy(x => x.LoginProvider).ToList();
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
            var user = new User(GlobalSettings, "Test", "test@test.com", "test", "helloworldtest");
            UserService.Save(user);

            ExternalLogin[] externalLogins = new[]
            {
                new ExternalLogin("test1", Guid.NewGuid().ToString("N")),
                new ExternalLogin("test2", Guid.NewGuid().ToString("N")),
                new ExternalLogin("test3", Guid.NewGuid().ToString("N")),
                new ExternalLogin("test4", Guid.NewGuid().ToString("N"))
            };

            ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ExternalLoginService.GetAll(user.Id).OrderBy(x => x.LoginProvider).ToList();

            logins.RemoveAt(0); // remove the first one
            logins.Add(new IdentityUserLogin("test5", Guid.NewGuid().ToString("N"), user.Id.ToString())); // add a new one

            // save new list
            ExternalLoginService.Save(user.Id, logins.Select(x => new ExternalLogin(x.LoginProvider, x.ProviderKey)));

            var updatedLogins = ExternalLoginService.GetAll(user.Id).OrderBy(x => x.LoginProvider).ToList();
            Assert.AreEqual(4, updatedLogins.Count);
            for (int i = 0; i < updatedLogins.Count; i++)
            {
                Assert.AreEqual(logins[i].LoginProvider, updatedLogins[i].LoginProvider);
            }
        }

        [Test]
        public void Add_Retrieve_User_Data()
        {
            var user = new User(GlobalSettings, "Test", "test@test.com", "test", "helloworldtest");
            UserService.Save(user);

            ExternalLogin[] externalLogins = new[]
            {
                new ExternalLogin("test1", Guid.NewGuid().ToString("N"), "hello world")
            };

            ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ExternalLoginService.GetAll(user.Id).ToList();

            Assert.AreEqual("hello world", logins[0].UserData);
        }
    }
}
