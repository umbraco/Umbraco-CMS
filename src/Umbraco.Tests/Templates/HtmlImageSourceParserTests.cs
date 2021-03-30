﻿using Umbraco.Core.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web.Templates;
using Umbraco.Web;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Routing;
using Umbraco.Tests.Testing.Objects;
using System.Web;
using System;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core;
using System.Diagnostics;

namespace Umbraco.Tests.Templates
{
    [TestFixture]
    public class HtmlImageSourceParserTests
    {
        [Test]
        public void Returns_Udis_From_Data_Udi_Html_Attributes()
        {
            var input = @"<p>
    <div>
        <img src='/media/12312.jpg' data-udi='umb://media/D4B18427A1544721B09AC7692F35C264' />
    </div>
</p><p><img src='/media/234234.jpg' data-udi=""umb://media-type/B726D735E4C446D58F703F3FBCFC97A5"" /></p>";

            var umbracoContextAccessor = new TestUmbracoContextAccessor();
            var imageSourceParser = new HtmlImageSourceParser(umbracoContextAccessor);

            var result = imageSourceParser.FindUdisFromDataAttributes(input).ToList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Udi.Parse("umb://media/D4B18427A1544721B09AC7692F35C264"), result[0]);
            Assert.AreEqual(Udi.Parse("umb://media-type/B726D735E4C446D58F703F3FBCFC97A5"), result[1]);
        }

        [Test]
        public void Remove_Image_Sources()
        {
            var umbracoContextAccessor = new TestUmbracoContextAccessor();
            var imageSourceParser = new HtmlImageSourceParser(umbracoContextAccessor);

            var result = imageSourceParser.RemoveImageSources(@"<p>
<div>
    <img src=""/media/12354/test.jpg"" />
</div></p>
<p>
    <div><img src=""/media/987645/test.jpg"" data-udi=""umb://media/81BB2036-034F-418B-B61F-C7160D68DCD4"" /></div>
</p>");

            Assert.AreEqual(@"<p>
<div>
    <img src=""/media/12354/test.jpg"" />
</div></p>
<p>
    <div><img src="""" data-udi=""umb://media/81BB2036-034F-418B-B61F-C7160D68DCD4"" /></div>
</p>", result);
        }

        [Test]
        public void Ensure_Image_Sources()
        {
            //setup a mock URL provider which we'll use for testing

            var mediaType = new PublishedContentType(Guid.NewGuid(), 777, "image", PublishedItemType.Media, Enumerable.Empty<string>(), Enumerable.Empty<PublishedPropertyType>(), ContentVariation.Nothing);
            var media = new Mock<IPublishedContent>();
            media.Setup(x => x.ContentType).Returns(mediaType);
            var mediaUrlProvider = new Mock<IMediaUrlProvider>();
            mediaUrlProvider.Setup(x => x.GetMediaUrl(It.IsAny<UmbracoContext>(), It.IsAny<IPublishedContent>(), It.IsAny<string>(), It.IsAny<UrlMode>(), It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(UrlInfo.Url("/media/1001/my-image.jpg"));

            var umbracoContextAccessor = new TestUmbracoContextAccessor();

            var umbracoContextFactory = TestUmbracoContextFactory.Create(
                mediaUrlProvider: mediaUrlProvider.Object,
                umbracoContextAccessor: umbracoContextAccessor);

            using (var reference = umbracoContextFactory.EnsureUmbracoContext(Mock.Of<HttpContextBase>()))
            {
                var mediaCache = Mock.Get(reference.UmbracoContext.Media);
                mediaCache.Setup(x => x.GetById(It.IsAny<Guid>())).Returns(media.Object);

                var imageSourceParser = new HtmlImageSourceParser(umbracoContextAccessor);

                var result = imageSourceParser.EnsureImageSources(@"<p>
<div>
    <img src="""" />
</div></p>
<p>
    <div><img src="""" data-udi=""umb://media/81BB2036-034F-418B-B61F-C7160D68DCD4"" /></div>
</p>
<p>
    <div><img src=""?width=100"" data-udi=""umb://media/81BB2036-034F-418B-B61F-C7160D68DCD4"" /></div>
</p>");

                Assert.AreEqual(@"<p>
<div>
    <img src="""" />
</div></p>
<p>
    <div><img src=""/media/1001/my-image.jpg"" data-udi=""umb://media/81BB2036-034F-418B-B61F-C7160D68DCD4"" /></div>
</p>
<p>
    <div><img src=""/media/1001/my-image.jpg?width=100"" data-udi=""umb://media/81BB2036-034F-418B-B61F-C7160D68DCD4"" /></div>
</p>", result);
            }
        }

        [TestCase(
            @"<div><img src="""" /></div>",
            ExpectedResult = @"<div><img src="""" /></div>",
            TestName = "Empty source is not updated with no data-udi set"
        )]
        [TestCase(
            @"<div><img src="""" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD4""/></div>",
            ExpectedResult = @"<div><img src=""/media/1001/image.jpg"" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD4""/></div>",
            TestName = "Empty source is updated with data-udi set"
        )]
        [TestCase(
            @"<div><img src=""non empty src"" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD4""/></div>",
            ExpectedResult = @"<div><img src=""/media/1001/image.jpg"" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD4""/></div>",
            TestName = "Filled source is overwritten with data-udi set"
        )]
        [TestCase(
            @"<div><img src=""some src"" some-attribute data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD4"" another-attribute/></div>",
            ExpectedResult = @"<div><img src=""/media/1001/image.jpg"" some-attribute data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD4"" another-attribute/></div>",
            TestName = "Attributes are persisted"
        )]
        [TestCase(
            @"<div><img src=""somesrc?width=100&height=500"" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD4""/></div>",
            ExpectedResult = @"<div><img src=""/media/1001/image.jpg?width=100&height=500"" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD4""/></div>",
            TestName = "Source is trimmed and parameters are prefixed"
        )]
        [TestCase(
            @"<div><img src=""?width=100&height=500"" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD4""/></div>",
            ExpectedResult = @"<div><img src=""/media/1001/image.jpg?width=100&height=500"" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD4""/></div>",
            TestName = "Parameters are prefixed"
        )]
        [TestCase(
            @"<div>
                <img src="""" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD4""/>
                <img src=""src"" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD5""/>
                <img src=""?asd"" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD6""/>
              </div>",
            ExpectedResult =
            @"<div>
                <img src=""/media/1001/image.jpg"" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD4""/>
                <img src=""/media/1001/image.jpg"" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD5""/>
                <img src=""/media/1001/image.jpg?asd"" data-udi=""umb://media/81BB2036034F418BB61FC7160D68DCD6""/>
              </div>",
            TestName = "Multiple img tags are handled"
        )]

        [Category("Ensure image sources")]
        public string Ensure_ImageSources_Processing(string sourceHtml)
        {
            var fakeMediaUrl = "/media/1001/image.jpg";
            var parser = new HtmlImageSourceParser((guid) => fakeMediaUrl);
            var actual = parser.EnsureImageSources(sourceHtml);

            return actual;
        }

        [Category("Ensure image sources")]
        [Test]
        public void Ensure_Large_Html_Is_Processed_Quickly()
        {
            int symbolCount = 25000;
            int maxMsToRun = 200;

            var longText = new string('*', symbolCount);
            var text = $@"<img src=""{longText}"" />";

            var fakeMediaUrl = "/media/1001/image.jpg";
            var parser = new HtmlImageSourceParser((guid) => fakeMediaUrl);

            var timer = new Stopwatch();
            timer.Start();
            var actual = parser.EnsureImageSources(text);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds <= maxMsToRun);
        }
    }
}
