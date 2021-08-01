using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Macros;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ContentElementTests : UmbracoSettingsTests
    {
        [Test]
        public void EmailAddress()
        {
            Assert.AreEqual("robot@umbraco.dk", SettingsSection.Content.NotificationEmailAddress);
        }
        [Test]
        public virtual void DisableHtmlEmail()
        {
            Assert.IsTrue(SettingsSection.Content.DisableHtmlEmail);
        }

        [Test]
        public virtual void Can_Set_Multiple()
        {
            Assert.AreEqual(3, SettingsSection.Content.Error404Collection.Count());
            Assert.AreEqual("default", SettingsSection.Content.Error404Collection.ElementAt(0).Culture);
            Assert.AreEqual(1047, SettingsSection.Content.Error404Collection.ElementAt(0).ContentId);
            Assert.IsTrue(SettingsSection.Content.Error404Collection.ElementAt(0).HasContentId);
            Assert.IsFalse(SettingsSection.Content.Error404Collection.ElementAt(0).HasContentKey);
            Assert.AreEqual("en-US", SettingsSection.Content.Error404Collection.ElementAt(1).Culture);
            Assert.AreEqual("$site/error [@name = 'error']", SettingsSection.Content.Error404Collection.ElementAt(1).ContentXPath);
            Assert.IsFalse(SettingsSection.Content.Error404Collection.ElementAt(1).HasContentId);
            Assert.IsFalse(SettingsSection.Content.Error404Collection.ElementAt(1).HasContentKey);
            Assert.AreEqual("en-UK", SettingsSection.Content.Error404Collection.ElementAt(2).Culture);
            Assert.AreEqual(new Guid("8560867F-B88F-4C74-A9A4-679D8E5B3BFC"), SettingsSection.Content.Error404Collection.ElementAt(2).ContentKey);
            Assert.IsTrue(SettingsSection.Content.Error404Collection.ElementAt(2).HasContentKey);
            Assert.IsFalse(SettingsSection.Content.Error404Collection.ElementAt(2).HasContentId);
        }

        [Test]
        public void ImageFileTypes()
        {
            Assert.IsTrue(SettingsSection.Content.ImageFileTypes.All(x => "jpeg,jpg,gif,bmp,png,tiff,tif".Split(',').Contains(x)));
        }

        [Test]
        public virtual void ImageAutoFillProperties()
        {
            Assert.AreEqual(2, SettingsSection.Content.ImageAutoFillProperties.Count());
            Assert.AreEqual("umbracoFile", SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).Alias);
            Assert.AreEqual("umbracoWidth", SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).WidthFieldAlias);
            Assert.AreEqual("umbracoHeight", SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).HeightFieldAlias);
            Assert.AreEqual("umbracoBytes", SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).LengthFieldAlias);
            Assert.AreEqual("umbracoExtension", SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).ExtensionFieldAlias);
            Assert.AreEqual("umbracoFile2", SettingsSection.Content.ImageAutoFillProperties.ElementAt(1).Alias);
            Assert.AreEqual("umbracoWidth2", SettingsSection.Content.ImageAutoFillProperties.ElementAt(1).WidthFieldAlias);
            Assert.AreEqual("umbracoHeight2", SettingsSection.Content.ImageAutoFillProperties.ElementAt(1).HeightFieldAlias);
            Assert.AreEqual("umbracoBytes2", SettingsSection.Content.ImageAutoFillProperties.ElementAt(1).LengthFieldAlias);
            Assert.AreEqual("umbracoExtension2", SettingsSection.Content.ImageAutoFillProperties.ElementAt(1).ExtensionFieldAlias);
        }

        [Test]
        public void PreviewBadge()
        {
            Assert.AreEqual(SettingsSection.Content.PreviewBadge, @"<div id=""umbracoPreviewBadge"" class=""umbraco-preview-badge""><span class=""umbraco-preview-badge__header"">Preview mode</span><a href=""{0}/preview/end?redir={1}"" class=""umbraco-preview-badge__end""><svg viewBox=""0 0 100 100"" xmlns=""http://www.w3.org/2000/svg""><title>Click to end</title><path d=""M5273.1 2400.1v-2c0-2.8-5-4-9.7-4s-9.7 1.3-9.7 4v2a7 7 0 002 4.9l5 4.9c.3.3.4.6.4 1v6.4c0 .4.2.7.6.8l2.9.9c.5.1 1-.2 1-.8v-7.2c0-.4.2-.7.4-1l5.1-5a7 7 0 002-4.9zm-9.7-.1c-4.8 0-7.4-1.3-7.5-1.8.1-.5 2.7-1.8 7.5-1.8s7.3 1.3 7.5 1.8c-.2.5-2.7 1.8-7.5 1.8z""/><path d=""M5268.4 2410.3c-.6 0-1 .4-1 1s.4 1 1 1h4.3c.6 0 1-.4 1-1s-.4-1-1-1h-4.3zM5272.7 2413.7h-4.3c-.6 0-1 .4-1 1s.4 1 1 1h4.3c.6 0 1-.4 1-1s-.4-1-1-1zM5272.7 2417h-4.3c-.6 0-1 .4-1 1s.4 1 1 1h4.3c.6 0 1-.4 1-1 0-.5-.4-1-1-1z""/><path d=""M78.2 13l-8.7 11.7a32.5 32.5 0 11-51.9 25.8c0-10.3 4.7-19.7 12.9-25.8L21.8 13a47 47 0 1056.4 0z""/><path d=""M42.7 2.5h14.6v49.4H42.7z""/></svg></a></div><style type=""text/css"">.umbraco-preview-badge {{position: absolute;top: 1em;right: 1em;display: inline-flex;background: #1b264f;color: #fff;padding: 1em;font-size: 12px;z-index: 99999999;justify-content: center;align-items: center;box-shadow: 0 10px 50px rgba(0, 0, 0, .1), 0 6px 20px rgba(0, 0, 0, .16);line-height: 1;}}.umbraco-preview-badge__header {{font-weight: bold;}}.umbraco-preview-badge__end {{width: 3em;padding: 1em;margin: -1em -1em -1em 2em;display: flex;flex-shrink: 0;align-items: center;align-self: stretch;}}.umbraco-preview-badge__end:hover,.umbraco-preview-badge__end:focus {{background: #f5c1bc;}}.umbraco-preview-badge__end svg {{fill: #fff;width:1em;}}</style>");
        }
        [Test]
        public void ResolveUrlsFromTextString()
        {
            Assert.IsFalse(SettingsSection.Content.ResolveUrlsFromTextString);
        }
        [Test]
        public void MacroErrors()
        {
            Assert.AreEqual(SettingsSection.Content.MacroErrorBehaviour, MacroErrorBehaviour.Inline);
        }

        [Test]
        public void DisallowedUploadFiles()
        {
            Assert.IsTrue(SettingsSection.Content.DisallowedUploadFiles.All(x => "ashx,aspx,ascx,config,cshtml,vbhtml,asmx,air,axd,xamlx".Split(',').Contains(x)));
        }

        [Test]
        public void AllowedUploadFiles()
        {
            Assert.IsTrue(SettingsSection.Content.AllowedUploadFiles.All(x => "jpg,gif,png".Split(',').Contains(x)));
        }

        [Test]
        [TestCase("png", true)]
        [TestCase("jpg", true)]
        [TestCase("gif", true)]
        // TODO: Why does it flip to TestingDefaults=true for these two tests on AppVeyor. WHY?
        //[TestCase("bmp", false)]
        //[TestCase("php", false)]
        [TestCase("ashx", false)]
        [TestCase("config", false)]
        public void IsFileAllowedForUpload_WithWhitelist(string extension, bool expected)
        {
            // Make really sure that defaults are NOT used
            TestingDefaults = false;

            Debug.WriteLine("Extension being tested", extension);
            Debug.WriteLine("AllowedUploadFiles: {0}", SettingsSection.Content.AllowedUploadFiles);
            Debug.WriteLine("DisallowedUploadFiles: {0}", SettingsSection.Content.DisallowedUploadFiles);

            var allowedContainsExtension = SettingsSection.Content.AllowedUploadFiles.Any(x => x.InvariantEquals(extension));
            var disallowedContainsExtension = SettingsSection.Content.DisallowedUploadFiles.Any(x => x.InvariantEquals(extension));

            Debug.WriteLine("AllowedContainsExtension: {0}", allowedContainsExtension);
            Debug.WriteLine("DisallowedContainsExtension: {0}", disallowedContainsExtension);

            Assert.AreEqual(expected, SettingsSection.Content.IsFileAllowedForUpload(extension));
        }
    }
}
