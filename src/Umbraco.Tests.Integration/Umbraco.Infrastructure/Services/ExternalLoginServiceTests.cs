// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class ExternalLoginServiceTests : UmbracoIntegrationTest
    {
        private IUserService UserService => GetRequiredService<IUserService>();

        private IExternalLoginService ExternalLoginService => (IExternalLoginService)GetRequiredService<IExternalLoginService>();

        [Test]
        [Ignore("We don't support duplicates anymore, this removing on save was a breaking change work around, this needs to be ported to a migration")]
        public void Removes_Existing_Duplicates_On_Save()
        {
            var user = new UserBuilder().Build();
            UserService.Save(user);

            string providerKey = Guid.NewGuid().ToString("N");
            DateTime latest = DateTime.Now.AddDays(-1);
            DateTime oldest = DateTime.Now.AddDays(-10);

            using (global::Umbraco.Cms.Core.Scoping.IScope scope = ScopeProvider.CreateScope())
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

            var logins = ExternalLoginService.GetExternalLogins(user.Id).ToList();

            // duplicates will be removed, keeping the latest entries
            Assert.AreEqual(2, logins.Count);

            IIdentityUserLogin test1 = logins.Single(x => x.LoginProvider == "test1");
            Assert.Greater(test1.CreateDate, latest);
        }

        [Test]
        public void Does_Not_Persist_Duplicates()
        {
            var user = new UserBuilder().Build();
            UserService.Save(user);

            string providerKey = Guid.NewGuid().ToString("N");
            ExternalLogin[] externalLogins = new[]
            {
                new ExternalLogin("test1", providerKey),
                new ExternalLogin("test1", providerKey)
            };

            ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ExternalLoginService.GetExternalLogins(user.Id).ToList();
            Assert.AreEqual(1, logins.Count);
        }

        [Test]
        public void Multiple_Update()
        {
            var user = new UserBuilder().Build();
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

            var found = ExternalLoginService.GetExternalLogins(user.Id).OrderBy(x => x.LoginProvider).ToList();
            Assert.AreEqual(2, found.Count);
            Assert.AreEqual("123456", found[0].UserData);
            Assert.AreEqual("987654", found[1].UserData);
        }

        [Test]
        public void Can_Find_As_Extended_Type()
        {
            var user = new UserBuilder().Build();
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
            var user = new UserBuilder().Build();
            UserService.Save(user);

            ExternalLogin[] externalLogins = new[]
            {
                new ExternalLogin("test1", Guid.NewGuid().ToString("N")),
                new ExternalLogin("test2", Guid.NewGuid().ToString("N"))
            };

            ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ExternalLoginService.GetExternalLogins(user.Id).OrderBy(x => x.LoginProvider).ToList();
            Assert.AreEqual(2, logins.Count);
            for (int i = 0; i < logins.Count; i++)
            {
                Assert.AreEqual(logins[i].ProviderKey, externalLogins[i].ProviderKey);
                Assert.AreEqual(logins[i].LoginProvider, externalLogins[i].LoginProvider);
            }
        }

        [Test]
        public void Add_Tokens()
        {
            var user = new UserBuilder().Build();
            UserService.Save(user);

            ExternalLogin[] externalLogins = new[]
            {
                new ExternalLogin("test1", Guid.NewGuid().ToString("N"))
            };

            ExternalLoginService.Save(user.Id, externalLogins);

            ExternalLoginToken[] externalTokens = new[]
            {
                new ExternalLoginToken(externalLogins[0].LoginProvider, "hello1", "world1"),
                new ExternalLoginToken(externalLogins[0].LoginProvider, "hello2", "world2")
            };

            ExternalLoginService.Save(user.Id, externalTokens);

            var tokens = ExternalLoginService.GetExternalLoginTokens(user.Id).ToList();
            Assert.AreEqual(2, tokens.Count);
        }

        [Test]
        public void Add_Update_Delete_Logins()
        {
            var user = new UserBuilder().Build();
            UserService.Save(user);

            ExternalLogin[] externalLogins = new[]
            {
                new ExternalLogin("test1", Guid.NewGuid().ToString("N")),
                new ExternalLogin("test2", Guid.NewGuid().ToString("N")),
                new ExternalLogin("test3", Guid.NewGuid().ToString("N")),
                new ExternalLogin("test4", Guid.NewGuid().ToString("N"))
            };

            ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ExternalLoginService.GetExternalLogins(user.Id).OrderBy(x => x.LoginProvider).ToList();

            logins.RemoveAt(0); // remove the first one
            logins.Add(new IdentityUserLogin("test5", Guid.NewGuid().ToString("N"), user.Id.ToString())); // add a new one
            logins[0].ProviderKey = "abcd123"; // update

            // save new list
            ExternalLoginService.Save(user.Id, logins.Select(x => new ExternalLogin(x.LoginProvider, x.ProviderKey)));

            var updatedLogins = ExternalLoginService.GetExternalLogins(user.Id).OrderBy(x => x.LoginProvider).ToList();
            Assert.AreEqual(4, updatedLogins.Count);
            for (int i = 0; i < updatedLogins.Count; i++)
            {
                Assert.AreEqual(logins[i].LoginProvider, updatedLogins[i].LoginProvider);
                Assert.AreEqual(logins[i].ProviderKey, updatedLogins[i].ProviderKey);
            }
        }

        [Test]
        public void Add_Update_Delete_Tokens()
        {
            var user = new UserBuilder().Build();
            UserService.Save(user);

            ExternalLogin[] externalLogins = new[]
            {
                new ExternalLogin("test1", Guid.NewGuid().ToString("N")),
                new ExternalLogin("test2", Guid.NewGuid().ToString("N"))
            };

            ExternalLoginService.Save(user.Id, externalLogins);

            ExternalLoginToken[] externalTokens = new[]
            {
                new ExternalLoginToken(externalLogins[0].LoginProvider, "hello1", "world1"),
                new ExternalLoginToken(externalLogins[0].LoginProvider, "hello1a", "world1a"),
                new ExternalLoginToken(externalLogins[1].LoginProvider, "hello2", "world2"),
                new ExternalLoginToken(externalLogins[1].LoginProvider, "hello2a", "world2a")
            };

            ExternalLoginService.Save(user.Id, externalTokens);

            var tokens = ExternalLoginService.GetExternalLoginTokens(user.Id).OrderBy(x => x.LoginProvider).ToList();

            tokens.RemoveAt(0); // remove the first one
            tokens.Add(new IdentityUserToken(externalLogins[1].LoginProvider, "hello2b", "world2b", user.Id.ToString())); // add a new one
            tokens[0].Value = "abcd123"; // update

            // save new list
            ExternalLoginService.Save(user.Id, tokens.Select(x => new ExternalLoginToken(x.LoginProvider, x.Name, x.Value)));

            var updatedTokens = ExternalLoginService.GetExternalLoginTokens(user.Id).OrderBy(x => x.LoginProvider).ToList();
            Assert.AreEqual(4, updatedTokens.Count);
            for (int i = 0; i < updatedTokens.Count; i++)
            {
                Assert.AreEqual(tokens[i].LoginProvider, updatedTokens[i].LoginProvider);
                Assert.AreEqual(tokens[i].Name, updatedTokens[i].Name);
                Assert.AreEqual(tokens[i].Value, updatedTokens[i].Value);
            }
        }

        [Test]
        public void Add_Retrieve_User_Data()
        {
            var user = new UserBuilder().Build();
            UserService.Save(user);

            ExternalLogin[] externalLogins = new[]
            {
                new ExternalLogin("test1", Guid.NewGuid().ToString("N"), "hello world")
            };

            ExternalLoginService.Save(user.Id, externalLogins);

            var logins = ExternalLoginService.GetExternalLogins(user.Id).ToList();

            Assert.AreEqual("hello world", logins[0].UserData);
        }
    }
}
