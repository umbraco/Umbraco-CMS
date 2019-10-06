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
            Assert.AreEqual(SettingsSection.Content.NotificationEmailAddress, "robot@umbraco.dk");
        }
        [Test]
        public virtual void DisableHtmlEmail()
        {
            Assert.IsTrue(SettingsSection.Content.DisableHtmlEmail);
        }

        [Test]
        public virtual void Can_Set_Multiple()
        {
            Assert.AreEqual(SettingsSection.Content.Error404Collection.Count(), 3);
            Assert.AreEqual(SettingsSection.Content.Error404Collection.ElementAt(0).Culture, "default");
            Assert.AreEqual(SettingsSection.Content.Error404Collection.ElementAt(0).ContentId, 1047);
            Assert.IsTrue(SettingsSection.Content.Error404Collection.ElementAt(0).HasContentId);
            Assert.IsFalse(SettingsSection.Content.Error404Collection.ElementAt(0).HasContentKey);
            Assert.AreEqual(SettingsSection.Content.Error404Collection.ElementAt(1).Culture, "en-US");
            Assert.AreEqual(SettingsSection.Content.Error404Collection.ElementAt(1).ContentXPath, "$site/error [@name = 'error']");
            Assert.IsFalse(SettingsSection.Content.Error404Collection.ElementAt(1).HasContentId);
            Assert.IsFalse(SettingsSection.Content.Error404Collection.ElementAt(1).HasContentKey);
            Assert.AreEqual(SettingsSection.Content.Error404Collection.ElementAt(2).Culture, "en-UK");
            Assert.AreEqual(SettingsSection.Content.Error404Collection.ElementAt(2).ContentKey, new Guid("8560867F-B88F-4C74-A9A4-679D8E5B3BFC"));
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
            Assert.AreEqual(SettingsSection.Content.ImageAutoFillProperties.Count(), 2);
            Assert.AreEqual(SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).Alias, "umbracoFile");
            Assert.AreEqual(SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).WidthFieldAlias, "umbracoWidth");
            Assert.AreEqual(SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).HeightFieldAlias, "umbracoHeight");
            Assert.AreEqual(SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).LengthFieldAlias, "umbracoBytes");
            Assert.AreEqual(SettingsSection.Content.ImageAutoFillProperties.ElementAt(0).ExtensionFieldAlias, "umbracoExtension");
            Assert.AreEqual(SettingsSection.Content.ImageAutoFillProperties.ElementAt(1).Alias, "umbracoFile2");
            Assert.AreEqual(SettingsSection.Content.ImageAutoFillProperties.ElementAt(1).WidthFieldAlias, "umbracoWidth2");
            Assert.AreEqual(SettingsSection.Content.ImageAutoFillProperties.ElementAt(1).HeightFieldAlias, "umbracoHeight2");
            Assert.AreEqual(SettingsSection.Content.ImageAutoFillProperties.ElementAt(1).LengthFieldAlias, "umbracoBytes2");
            Assert.AreEqual(SettingsSection.Content.ImageAutoFillProperties.ElementAt(1).ExtensionFieldAlias, "umbracoExtension2");
        }
        
        [Test]
        public void PreviewBadge()
        {
            Assert.AreEqual(SettingsSection.Content.PreviewBadge, @"<div id=""umbracoPreviewBadge"" class=""umbraco-preview-badge""><span class=""umbraco-preview-badge__header"">Preview mode</span><a href=""{0}/preview/end?redir={1}"" class=""umbraco-preview-badge__end""><svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 357 357""><title>Click to end</title><path d=""M357 35.7L321.3 0 178.5 142.8 35.7 0 0 35.7l142.8 142.8L0 321.3 35.7 357l142.8-142.8L321.3 357l35.7-35.7-142.8-142.8z""></path></svg></a></div><style type=""text/css"">.umbraco-preview-badge {{position: absolute;top: 1em;right: 1em;display: inline-flex;background: #1b264f;color: #fff;padding: 1em;font-size: 12px;z-index: 99999999;justify-content: center;align-items: center;box-shadow: 0 10px 50px rgba(0, 0, 0, .1), 0 6px 20px rgba(0, 0, 0, .16);line-height: 1;}}.umbraco-preview-badge__header {{font-weight: bold;}}.umbraco-preview-badge__end {{width: 3em;padding: 1em;margin: -1em -1em -1em 2em;display: flex;align-items: center;align-self: stretch;}}.umbraco-preview-badge__end:hover,.umbraco-preview-badge__end:focus {{background: #f5c1bc;}}.umbraco-preview-badge__end svg {{fill: #fff;}}</style>");
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
            Assert.IsTrue(SettingsSection.Content.DisallowedUploadFiles.All(x => "ashx,aspx,ascx,config,cshtml,vbhtml,asmx,air,axd".Split(',').Contains(x)));
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

            Assert.AreEqual(SettingsSection.Content.IsFileAllowedForUpload(extension), expected);
        }
    }
}
