using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
    public class ImageCropperTest
    {
        private const string cropperJson = "{\"focalPoint\": {\"left\": 0.96,\"top\": 0.80827067669172936},\"src\": \"/media/1005/img_0671.jpg\",\"crops\": [{\"alias\":\"thumb\",\"width\": 100,\"height\": 100,\"coordinates\": {\"x1\": 0.58729977382575338,\"y1\": 0.055768992440203169,\"x2\": 0,\"y2\": 0.32457553600198386}}]}";

        private const string mediaPath = "/media/1005/img_0671.jpg";

        [Test]
        public void GetCropUrl_CropAliasTest()
        {
            var urlString = mediaPath.GetCropUrl(imageCropperValue: cropperJson, cropAlias: "Thumb", useCropDimensions: true);
            Assert.AreEqual(mediaPath + "?crop=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&cropmode=percentage&width=100&height=100", urlString);
        }

        [Test]
        public void GetCropUrl_WidthHeightTest()
        {
            var urlString = mediaPath.GetCropUrl(imageCropperValue: cropperJson, width: 200, height: 300);
            Assert.AreEqual(mediaPath + "?center=0.80827067669172936,0.96&mode=crop&width=200&height=300", urlString);
        }

        [Test]
        public void GetCropUrl_FocalPointTest()
        {
            var urlString = mediaPath.GetCropUrl(imageCropperValue: cropperJson, cropAlias: "thumb", preferFocalPoint: true, useCropDimensions: true);
            Assert.AreEqual(mediaPath + "?center=0.80827067669172936,0.96&mode=crop&width=100&height=100", urlString);
        }

        [Test]
        public void GetCropUrlFurtherOptionsTest()
        {
            var urlString = mediaPath.GetCropUrl(imageCropperValue: cropperJson, width: 200, height: 300, furtherOptions: "&filter=comic&roundedcorners=radius-26|bgcolor-fff");
            Assert.AreEqual(mediaPath + "?center=0.80827067669172936,0.96&mode=crop&width=200&height=300&filter=comic&roundedcorners=radius-26|bgcolor-fff", urlString);
        }

    }
}