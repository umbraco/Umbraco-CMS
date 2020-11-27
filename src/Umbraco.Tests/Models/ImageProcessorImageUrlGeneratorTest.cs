using System;
using System.Globalization;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using Umbraco.Tests.Components;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Models;
using Umbraco.Web;
using Umbraco.Web.PropertyEditors;
using System.Text;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class ImageProcessorImageUrlGeneratorTest
    {
        private const string MediaPath = "/media/1005/img_0671.jpg";
        private static readonly ImageUrlGenerationOptions.CropCoordinates Crop = new ImageUrlGenerationOptions.CropCoordinates(0.58729977382575338m, 0.055768992440203169m, 0m, 0.32457553600198386m);
        private static readonly ImageUrlGenerationOptions.FocalPointPosition Focus1 = new ImageUrlGenerationOptions.FocalPointPosition(0.80827067669172936m, 0.96m);
        private static readonly ImageUrlGenerationOptions.FocalPointPosition Focus2 = new ImageUrlGenerationOptions.FocalPointPosition(0.41m, 0.4275m);
        private static readonly ImageProcessorImageUrlGenerator Generator = new ImageProcessorImageUrlGenerator();

        [Test]
        public void GetCropUrl_CropAliasTest()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { Crop = Crop, Width = 100, Height = 100 });
            Assert.AreEqual(MediaPath + "?crop=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&cropmode=percentage&width=100&height=100", urlString);
        }

        [Test]
        public void GetCropUrl_WidthHeightTest()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { FocalPoint = Focus1, Width = 200, Height = 300 });
            Assert.AreEqual(MediaPath + "?center=0.80827067669172936,0.96&mode=crop&width=200&height=300", urlString);
        }

        [Test]
        public void GetCropUrl_FocalPointTest()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { FocalPoint = Focus1, Width = 100, Height = 100 });
            Assert.AreEqual(MediaPath + "?center=0.80827067669172936,0.96&mode=crop&width=100&height=100", urlString);
        }

        [Test]
        public void GetCropUrlFurtherOptionsTest()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { FocalPoint = Focus1, Width = 200, Height = 300, FurtherOptions = "&filter=comic&roundedcorners=radius-26|bgcolor-fff" });
            Assert.AreEqual(MediaPath + "?center=0.80827067669172936,0.96&mode=crop&width=200&height=300&filter=comic&roundedcorners=radius-26|bgcolor-fff", urlString);
        }

        /// <summary>
        /// Test that if a crop alias has been specified that doesn't exist the method returns null
        /// </summary>
        [Test]
        public void GetCropUrlNullTest()
        {
            var urlString = Generator.GetImageUrl(null);
            Assert.AreEqual(null, urlString);
        }

        /// <summary>
        /// Test that if a crop alias has been specified that doesn't exist the method returns null
        /// </summary>
        [Test]
        public void GetCropUrlEmptyTest()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(null));
            Assert.AreEqual("?mode=crop", urlString);
        }

        /// <summary>
        /// Test the GetCropUrl method on the ImageCropDataSet Model
        /// </summary>
        [Test]
        public void GetBaseCropUrlFromModelTest()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(null) { Crop = Crop, Width = 100, Height = 100 });
            Assert.AreEqual("?crop=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&cropmode=percentage&width=100&height=100", urlString);
        }

        /// <summary>
        /// Test the height ratio mode with predefined crop dimensions
        /// </summary>
        [Test]
        public void GetCropUrl_CropAliasHeightRatioModeTest()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { Crop = Crop, Width = 100, HeightRatio = 1 });
            Assert.AreEqual(MediaPath + "?crop=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&cropmode=percentage&heightratio=1&width=100", urlString);
        }

        /// <summary>
        /// Test the height ratio mode with manual width/height dimensions
        /// </summary>
        [Test]
        public void GetCropUrl_WidthHeightRatioModeTest()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { FocalPoint = Focus1, Width = 300, HeightRatio = 0.5m });
            Assert.AreEqual(MediaPath + "?center=0.80827067669172936,0.96&mode=crop&heightratio=0.5&width=300", urlString);
        }

        /// <summary>
        /// Test the height ratio mode with width/height dimensions
        /// </summary>
        [Test]
        public void GetCropUrl_HeightWidthRatioModeTest()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { FocalPoint = Focus1, Height = 150, WidthRatio = 2 });
            Assert.AreEqual(MediaPath + "?center=0.80827067669172936,0.96&mode=crop&widthratio=2&height=150", urlString);
        }

        /// <summary>
        /// Test that if Crop mode is specified as anything other than Crop the image doesn't use the crop
        /// </summary>
        [Test]
        public void GetCropUrl_SpecifiedCropModeTest()
        {
            var urlStringMin = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { ImageCropMode = "Min", Width = 300, Height = 150 });
            var urlStringBoxPad = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { ImageCropMode = "BoxPad", Width = 300, Height = 150 });
            var urlStringPad = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { ImageCropMode = "Pad", Width = 300, Height = 150 });
            var urlStringMax = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { ImageCropMode = "Max", Width = 300, Height = 150 });
            var urlStringStretch = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { ImageCropMode = "Stretch", Width = 300, Height = 150 });

            Assert.AreEqual(MediaPath + "?mode=min&width=300&height=150", urlStringMin);
            Assert.AreEqual(MediaPath + "?mode=boxpad&width=300&height=150", urlStringBoxPad);
            Assert.AreEqual(MediaPath + "?mode=pad&width=300&height=150", urlStringPad);
            Assert.AreEqual(MediaPath + "?mode=max&width=300&height=150", urlStringMax);
            Assert.AreEqual(MediaPath + "?mode=stretch&width=300&height=150", urlStringStretch);
        }

        /// <summary>
        /// Test for upload property type
        /// </summary>
        [Test]
        public void GetCropUrl_UploadTypeTest()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { ImageCropMode = "Crop", ImageCropAnchor = "Center", Width = 100, Height = 270 });
            Assert.AreEqual(MediaPath + "?mode=crop&anchor=center&width=100&height=270", urlString);
        }

        /// <summary>
        /// Test for preferFocalPoint when focal point is centered
        /// </summary>
        [Test]
        public void GetCropUrl_PreferFocalPointCenter()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { DefaultCrop = true, Width = 300, Height = 150 });
            Assert.AreEqual(MediaPath + "?anchor=center&mode=crop&width=300&height=150", urlString);
        }

        /// <summary>
        /// Test to check if height ratio is returned for a predefined crop without coordinates and focal point in centre when a width parameter is passed
        /// </summary>
        [Test]
        public void GetCropUrl_PreDefinedCropNoCoordinatesWithWidth()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { DefaultCrop = true, Width = 200, HeightRatio = 0.5962962962962962962962962963m });
            Assert.AreEqual(MediaPath + "?anchor=center&mode=crop&heightratio=0.5962962962962962962962962963&width=200", urlString);
        }

        /// <summary>
        /// Test to check if height ratio is returned for a predefined crop without coordinates and focal point is custom when a width parameter is passed
        /// </summary>
        [Test]
        public void GetCropUrl_PreDefinedCropNoCoordinatesWithWidthAndFocalPoint()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { FocalPoint = Focus2, Width = 200, HeightRatio = 0.5962962962962962962962962963m });
            Assert.AreEqual(MediaPath + "?center=0.41,0.4275&mode=crop&heightratio=0.5962962962962962962962962963&width=200", urlString);
        }

        /// <summary>
        /// Test to check if crop ratio is ignored if useCropDimensions is true
        /// </summary>
        [Test]
        public void GetCropUrl_PreDefinedCropNoCoordinatesWithWidthAndFocalPointIgnore()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { FocalPoint = Focus2, Width = 270, Height = 161 });
            Assert.AreEqual(MediaPath + "?center=0.41,0.4275&mode=crop&width=270&height=161", urlString);
        }

        /// <summary>
        /// Test to check if width ratio is returned for a predefined crop without coordinates and focal point in centre when a height parameter is passed
        /// </summary>
        [Test]
        public void GetCropUrl_PreDefinedCropNoCoordinatesWithHeight()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { DefaultCrop = true, Height = 200, WidthRatio = 1.6770186335403726708074534161m });
            Assert.AreEqual(MediaPath + "?anchor=center&mode=crop&widthratio=1.6770186335403726708074534161&height=200", urlString);
        }

        /// <summary>
        /// Test to check result when only a width parameter is passed, effectivly a resize only
        /// </summary>
        [Test]
        public void GetCropUrl_WidthOnlyParameter()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { DefaultCrop = true, Width = 200 });
            Assert.AreEqual(MediaPath + "?anchor=center&mode=crop&width=200", urlString);
        }

        /// <summary>
        /// Test to check result when only a height parameter is passed, effectivly a resize only
        /// </summary>
        [Test]
        public void GetCropUrl_HeightOnlyParameter()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { DefaultCrop = true, Height = 200 });
            Assert.AreEqual(MediaPath + "?anchor=center&mode=crop&height=200", urlString);
        }

        /// <summary>
        /// Test to check result when using a background color with padding
        /// </summary>
        [Test]
        public void GetCropUrl_BackgroundColorParameter()
        {
            var urlString = Generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { ImageCropMode = "Pad", Width = 400, Height = 400, FurtherOptions = "&bgcolor=fff" });
            Assert.AreEqual(MediaPath + "?mode=pad&width=400&height=400&bgcolor=fff", urlString);
        }
    }
}
