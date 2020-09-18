﻿using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using Umbraco.Core.Models;
using Umbraco.Tests.Testing;
using Current = Umbraco.Web.Composing.Current;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class ContentFinderByUrlAndTemplateTests : BaseWebTest
    {
        Template CreateTemplate(string alias)
        {
            var template = new Template(ShortStringHelper, alias, alias);
            template.Content = ""; // else saving throws with a dirty internal error
            ServiceContext.FileService.SaveTemplate(template);
            return template;
        }

        [TestCase("/blah")]
        [TestCase("/default.aspx/blah")] //this one is actually rather important since this is the path that comes through when we are running in pre-IIS 7 for the root document '/' !
        [TestCase("/home/Sub1/blah")]
        [TestCase("/Home/Sub1/Blah")] //different cases
        [TestCase("/home/Sub1.aspx/blah")]
        public void Match_Document_By_Url_With_Template(string urlAsString)
        {
            var globalSettings = new GlobalSettingsBuilder()
                .WithHideTopLevelNodeFromPath(false)
                .Build();

            var template1 = CreateTemplate("test");
            var template2 = CreateTemplate("blah");
            var umbracoContext = GetUmbracoContext(urlAsString, template1.Id, globalSettings: globalSettings);
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            var webRoutingSettings = new WebRoutingSettingsBuilder().Build();
            var lookup = new ContentFinderByUrlAndTemplate(Logger, ServiceContext.FileService, ServiceContext.ContentTypeService, Microsoft.Extensions.Options.Options.Create(webRoutingSettings));

            var result = lookup.TryFindContent(frequest);

            Assert.IsTrue(result);
            Assert.IsNotNull(frequest.PublishedContent);
            Assert.IsNotNull(frequest.TemplateAlias);
            Assert.AreEqual("blah".ToUpperInvariant(), frequest.TemplateAlias.ToUpperInvariant());
        }
    }
}
