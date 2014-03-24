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
        [Test]
        public void GetCropUrl_Test()
        {
            var cropperJson =
                "{\"src\": \"/media/1001/img_0671.jpg\",\"crops\": [{\"alias\": \"Thumb\",\"width\": 100,\"height\": 100,\"coordinates\": {\"x1\": 0.36664230810213955,\"y1\": 0.25767203967761981,\"x2\": 0.48805391012476662,\"y2\": 0.54858958462492169}}],\"focalPoint\": {\"left\": 0.8075,\"top\": 0.8666666666666667}}";

            var urlString = "/media/1001/img_0671.jpg".GetCropUrl(imageCropperValue:cropperJson, cropAlias:"Thumb", width:100, height:100);

            Assert.AreEqual(urlString, "/media/1001/img_0671.jpg?crop=0.36664230810213955,0.25767203967761981,0.48805391012476662,0.54858958462492169&cropmode=percentage&width=100&height=100");
        }

    }
}