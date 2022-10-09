using System;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.BackOffice.Install;
using Umbraco.Cms.Web.BackOffice.Routing;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.AutoFixture.Customizations;

internal class UmbracoCustomizations : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<BackOfficeIdentityUser>(
            u => u.FromFactory<string, string, string>(
                (a, b, c) => BackOfficeIdentityUser.CreateNew(new GlobalSettings(), a, b, c)));

        fixture
            .Customize(new ConstructorCustomization(typeof(UsersController), new GreedyConstructorQuery()))
            .Customize(new ConstructorCustomization(typeof(InstallController), new GreedyConstructorQuery()))
            .Customize(new ConstructorCustomization(typeof(PreviewController), new GreedyConstructorQuery()))
            .Customize(new ConstructorCustomization(typeof(MemberController), new GreedyConstructorQuery()))
            .Customize(new ConstructorCustomization(typeof(BackOfficeController), new GreedyConstructorQuery()))
            .Customize(new ConstructorCustomization(typeof(BackOfficeUserManager), new GreedyConstructorQuery()))
            .Customize(new ConstructorCustomization(typeof(MemberManager), new GreedyConstructorQuery()))
            .Customize(new ConstructorCustomization(typeof(DatabaseSchemaCreatorFactory), new GreedyConstructorQuery()))
            .Customize(new ConstructorCustomization(typeof(BackOfficeServerVariables), new GreedyConstructorQuery()))
            .Customize(new ConstructorCustomization(typeof(InstallHelper), new GreedyConstructorQuery()));

        // When requesting an IUserStore ensure we actually uses a IUserLockoutStore
        fixture.Customize<IUserStore<BackOfficeIdentityUser>>(cc =>
            cc.FromFactory(Mock.Of<IUserLockoutStore<BackOfficeIdentityUser>>));

        fixture.Customize<IUmbracoVersion>(
            u => u.FromFactory(
                () => new UmbracoVersion()));

        fixture.Customize<HostingSettings>(x =>
            x.With(settings => settings.ApplicationVirtualPath, string.Empty));

        fixture.Customize<BackOfficeAreaRoutes>(u => u.FromFactory(
            () => new BackOfficeAreaRoutes(
                Options.Create(new GlobalSettings()),
                Mock.Of<IHostingEnvironment>(x =>
                    x.ToAbsolute(It.IsAny<string>()) == "/umbraco" && x.ApplicationVirtualPath == string.Empty),
                Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run),
                new UmbracoApiControllerTypeCollection(Enumerable.Empty<Type>))));

        fixture.Customize<PreviewRoutes>(u => u.FromFactory(
            () => new PreviewRoutes(
                Options.Create(new GlobalSettings()),
                Mock.Of<IHostingEnvironment>(x =>
                    x.ToAbsolute(It.IsAny<string>()) == "/umbraco" && x.ApplicationVirtualPath == string.Empty),
                Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run))));

        var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        fixture.Customize<HttpContext>(x => x.FromFactory(() => httpContextAccessor.HttpContext));
        fixture.Customize<IHttpContextAccessor>(x => x.FromFactory(() => httpContextAccessor));

        fixture.Customize<WebRoutingSettings>(x =>
            x.With(settings => settings.UmbracoApplicationUrl, "http://localhost:5000"));
    }
}
