using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ContentElementTests : UmbracoSettingsTests
    {
        [Test]
        public void EmailAddress()
        {
            Assert.IsTrue(Section.Content.NotificationEmailAddress == "robot@umbraco.dk");
        }
        [Test]
        public virtual void DisableHtmlEmail()
        {
            Assert.IsTrue(Section.Content.DisableHtmlEmail == true);
        }

        [Test]
        public virtual void Can_Set_Multiple()
        {
            Assert.IsTrue(Section.Content.Error404Collection.Count() == 3);
            Assert.IsTrue(Section.Content.Error404Collection.ElementAt(0).Culture == "default");
            Assert.IsTrue(Section.Content.Error404Collection.ElementAt(0).ContentId == 1047);
            Assert.IsTrue(Section.Content.Error404Collection.ElementAt(1).Culture == "en-US");
            Assert.IsTrue(Section.Content.Error404Collection.ElementAt(1).ContentId == 1048);
            Assert.IsTrue(Section.Content.Error404Collection.ElementAt(2).Culture == "en-UK");
            Assert.IsTrue(Section.Content.Error404Collection.ElementAt(2).ContentId == 1049);
        }    

        [Test]
        public void ScriptFolderPath()
        {
            Assert.IsTrue(Section.Content.ScriptFolderPath == "/scripts");
        }
        [Test]
        public void ScriptFileTypes()
        {
            Assert.IsTrue(Section.Content.ScriptFileTypes.All(x => "js,xml".Split(',').Contains(x)));
        }
        [Test]
        public void DisableScriptEditor()
        {
            Assert.IsTrue(Section.Content.ScriptEditorDisable == false);
        }

        [Test]
        public void ImageFileTypes()
        {
            Assert.IsTrue(Section.Content.ImageFileTypes.All(x => "jpeg,jpg,gif,bmp,png,tiff,tif".Split(',').Contains(x)));
        }
        [Test]
        public void AllowedAttributes()
        {
            Assert.IsTrue(Section.Content.ImageTagAllowedAttributes.All(x => "src,alt,border,class,style,align,id,name,onclick,usemap".Split(',').Contains(x)));
        }
        [Test]
        public virtual void ImageAutoFillProperties()
        {
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.Count() == 2);
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(0).Alias == "umbracoFile");
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(0).WidthFieldAlias == "umbracoWidth");
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(0).HeightFieldAlias == "umbracoHeight");
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(0).LengthFieldAlias == "umbracoBytes");
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(0).ExtensionFieldAlias == "umbracoExtension");
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(1).Alias == "umbracoFile2");
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(1).WidthFieldAlias == "umbracoWidth2");
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(1).HeightFieldAlias == "umbracoHeight2");
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(1).LengthFieldAlias == "umbracoBytes2");
            Assert.IsTrue(Section.Content.ImageAutoFillProperties.ElementAt(1).ExtensionFieldAlias == "umbracoExtension2");
        }

        [Test]
        public void UploadAllowDirectories()
        {
            Assert.IsTrue(Section.Content.UploadAllowDirectories == true);
        }
        [Test]
        public void DefaultDocumentTypeProperty()
        {
            Assert.IsTrue(Section.Content.DefaultDocumentTypeProperty == "Textstring");
        }
        [Test]
        public void GlobalPreviewStorageEnabled()
        {
            Assert.IsTrue(Section.Content.GlobalPreviewStorageEnabled == false);
        }
        [Test]
        public void CloneXmlContent()
        {
            Assert.IsTrue(Section.Content.CloneXmlContent == true);
        }
        [Test]
        public void EnsureUniqueNaming()
        {
            Assert.IsTrue(Section.Content.EnsureUniqueNaming == true);
        }
        [Test]
        public void TidyEditorContent()
        {
            Assert.IsTrue(Section.Content.TidyEditorContent == false);
        }
        [Test]
        public virtual void TidyCharEncoding()
        {
            Assert.IsTrue(Section.Content.TidyCharEncoding == "Raw");
        }
        [Test]
        public void UseLegacyXmlSchema()
        {
            Assert.IsTrue(Section.Content.UseLegacyXmlSchema == false);
        }
        [Test]
        public void ForceSafeAliases()
        {
            Assert.IsTrue(Section.Content.ForceSafeAliases == true);
        }
        [Test]
        public void XmlCacheEnabled()
        {
            Assert.IsTrue(Section.Content.XmlCacheEnabled == true);
        }
        [Test]
        public void ContinouslyUpdateXmlDiskCache()
        {
            Assert.IsTrue(Section.Content.ContinouslyUpdateXmlDiskCache == true);
        }
        [Test]
        public virtual void XmlContentCheckForDiskChanges()
        {
            Assert.IsTrue(Section.Content.XmlContentCheckForDiskChanges == true);
        }
        [Test]
        public void EnableSplashWhileLoading()
        {
            Assert.IsTrue(Section.Content.EnableSplashWhileLoading == false);
        }
        [Test]
        public void PropertyContextHelpOption()
        {
            Assert.IsTrue(Section.Content.PropertyContextHelpOption == "text");
        }
        [Test]
        public void EnableCanvasEditing()
        {
            Assert.IsTrue(Section.Content.EnableCanvasEditing == false);
        }
        [Test]
        public void PreviewBadge()
        {
            Assert.IsTrue(Section.Content.PreviewBadge == @"<a id=""umbracoPreviewBadge"" style=""position: absolute; top: 0; right: 0; border: 0; width: 149px; height: 149px; background: url('{1}/preview/previewModeBadge.png') no-repeat;"" href=""{0}/endPreview.aspx?redir={2}""><span style=""display:none;"">In Preview Mode - click to end</span></a>");
        }
        [Test]
        public void UmbracoLibraryCacheDuration()
        {
            Assert.IsTrue(Section.Content.UmbracoLibraryCacheDuration == 1800);
        }
        [Test]
        public void ResolveUrlsFromTextString()
        {
            Assert.IsTrue(Section.Content.ResolveUrlsFromTextString);
        }
        [Test]
        public void MacroErrors()
        {
            Assert.IsTrue(Section.Content.MacroErrorBehaviour == MacroErrorBehaviour.Inline);
        }
        [Test]
        public void DocumentTypeIconList()
        {
            Assert.IsTrue(Section.Content.IconPickerBehaviour == IconPickerBehaviour.HideFileDuplicates);
        }
        [Test]
        public void DisallowedUploadFiles()
        {
            Assert.IsTrue(Section.Content.DisallowedUploadFiles.All(x => "ashx,aspx,ascx,config,cshtml,vbhtml,asmx,air,axd".Split(',').Contains(x)));
        }
    }
}
