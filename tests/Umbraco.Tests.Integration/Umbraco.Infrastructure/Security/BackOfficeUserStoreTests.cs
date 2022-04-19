using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Security
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class BackOfficeUserStoreTests : UmbracoIntegrationTest
    {
        private IUserService UserService => GetRequiredService<IUserService>();
        private IEntityService EntityService => GetRequiredService<IEntityService>();
        private IExternalLoginWithKeyService ExternalLoginService => GetRequiredService<IExternalLoginWithKeyService>();
        private IUmbracoMapper UmbracoMapper => GetRequiredService<IUmbracoMapper>();
        private ILocalizedTextService TextService => GetRequiredService<ILocalizedTextService>();
        private ITwoFactorLoginService TwoFactorLoginService => GetRequiredService<ITwoFactorLoginService>();

        private BackOfficeUserStore GetUserStore()
            => new BackOfficeUserStore(
                    ScopeProvider,
                    UserService,
                    EntityService,
                    ExternalLoginService,
                    Options.Create(GlobalSettings),
                    UmbracoMapper,
                    new BackOfficeErrorDescriber(TextService),
                    AppCaches,
                    TwoFactorLoginService
                    );

        [Test]
        public async Task Can_Persist_Is_Approved()
        {
            var userStore = GetUserStore();
            var user = new BackOfficeIdentityUser(GlobalSettings, 1, new List<IReadOnlyUserGroup>())
            {
                Name = "Test",
                Email = "test@test.com",
                UserName = "test@test.com"
            };
            IdentityResult createResult = await userStore.CreateAsync(user);
            Assert.IsTrue(createResult.Succeeded);
            Assert.IsFalse(user.IsApproved);

            // update
            user.IsApproved = true;
            var saveResult = await userStore.UpdateAsync(user);
            Assert.IsTrue(saveResult.Succeeded);
            Assert.IsTrue(user.IsApproved);

            // get get
            user = await userStore.FindByIdAsync(user.Id);
            Assert.IsTrue(user.IsApproved);
        }
    }
}
