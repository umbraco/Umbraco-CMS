using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Dynamics;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class DynamicXmlConverterTests 
    {
        [Test]
        public void Convert_To_Raw_String()
        {
            var xml = "<DAMP fullMedia=\"\" test-attribute=\"someValue\"><mediaItem><Image id=\"1057\" version=\"d58d5c16-153e-4896-892f-a722e45a69af\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:55:29\" updateDate=\"2012-11-05T16:55:44\" nodeName=\"test12\" urlName=\"test12\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1057\"><umbracoFile>/media/54/tulips.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>620888</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:55:34\"><crop name=\"thumbCrop\" x=\"154\" y=\"1\" x2=\"922\" y2=\"768\" url=\"/media/54/tulips_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem><mediaItem><Image id=\"1055\" version=\"4df1f08a-3552-45f2-b4bf-fa980c762f4a\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:29:58\" updateDate=\"2012-11-05T16:30:27\" nodeName=\"Test\" urlName=\"test\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1055\"><umbracoFile>/media/41/hydrangeas.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>595284</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:30:18\"><crop name=\"thumbCrop\" x=\"133\" y=\"0\" x2=\"902\" y2=\"768\" url=\"/media/41/hydrangeas_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem></DAMP>";
            var dXml = new DynamicXml(
                XmlHelper.StripDashesInElementOrAttributeNames(xml),
                xml);
            var result = dXml.TryConvertTo<RawXmlString>();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(xml, result.Result.Value);
        }

        [Test]
        public void Convert_To_Raw_XElement()
        {
            var xml = "<DAMP fullMedia=\"\" test-attribute=\"someValue\"><mediaItem><Image id=\"1057\" version=\"d58d5c16-153e-4896-892f-a722e45a69af\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:55:29\" updateDate=\"2012-11-05T16:55:44\" nodeName=\"test12\" urlName=\"test12\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1057\"><umbracoFile>/media/54/tulips.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>620888</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:55:34\"><crop name=\"thumbCrop\" x=\"154\" y=\"1\" x2=\"922\" y2=\"768\" url=\"/media/54/tulips_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem><mediaItem><Image id=\"1055\" version=\"4df1f08a-3552-45f2-b4bf-fa980c762f4a\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:29:58\" updateDate=\"2012-11-05T16:30:27\" nodeName=\"Test\" urlName=\"test\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1055\"><umbracoFile>/media/41/hydrangeas.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>595284</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:30:18\"><crop name=\"thumbCrop\" x=\"133\" y=\"0\" x2=\"902\" y2=\"768\" url=\"/media/41/hydrangeas_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem></DAMP>";
            var dXml = new DynamicXml(
                XmlHelper.StripDashesInElementOrAttributeNames(xml),
                xml);
            var result = dXml.TryConvertTo<RawXElement>();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(xml, result.Result.Value.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void Convert_To_Raw_XmlElement()
        {
            var xml = "<DAMP fullMedia=\"\" test-attribute=\"someValue\"><mediaItem><Image id=\"1057\" version=\"d58d5c16-153e-4896-892f-a722e45a69af\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:55:29\" updateDate=\"2012-11-05T16:55:44\" nodeName=\"test12\" urlName=\"test12\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1057\"><umbracoFile>/media/54/tulips.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>620888</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:55:34\"><crop name=\"thumbCrop\" x=\"154\" y=\"1\" x2=\"922\" y2=\"768\" url=\"/media/54/tulips_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem><mediaItem><Image id=\"1055\" version=\"4df1f08a-3552-45f2-b4bf-fa980c762f4a\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:29:58\" updateDate=\"2012-11-05T16:30:27\" nodeName=\"Test\" urlName=\"test\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1055\"><umbracoFile>/media/41/hydrangeas.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>595284</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:30:18\"><crop name=\"thumbCrop\" x=\"133\" y=\"0\" x2=\"902\" y2=\"768\" url=\"/media/41/hydrangeas_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem></DAMP>";
            var dXml = new DynamicXml(
                XmlHelper.StripDashesInElementOrAttributeNames(xml),
                xml);
            var result = dXml.TryConvertTo<RawXmlElement>();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(xml, result.Result.Value.OuterXml);
        }

        [Test]
        public void Convert_To_Raw_XmlDocument()
        {
            var xml = "<DAMP fullMedia=\"\" test-attribute=\"someValue\"><mediaItem><Image id=\"1057\" version=\"d58d5c16-153e-4896-892f-a722e45a69af\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:55:29\" updateDate=\"2012-11-05T16:55:44\" nodeName=\"test12\" urlName=\"test12\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1057\"><umbracoFile>/media/54/tulips.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>620888</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:55:34\"><crop name=\"thumbCrop\" x=\"154\" y=\"1\" x2=\"922\" y2=\"768\" url=\"/media/54/tulips_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem><mediaItem><Image id=\"1055\" version=\"4df1f08a-3552-45f2-b4bf-fa980c762f4a\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:29:58\" updateDate=\"2012-11-05T16:30:27\" nodeName=\"Test\" urlName=\"test\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1055\"><umbracoFile>/media/41/hydrangeas.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>595284</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:30:18\"><crop name=\"thumbCrop\" x=\"133\" y=\"0\" x2=\"902\" y2=\"768\" url=\"/media/41/hydrangeas_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem></DAMP>";
            var dXml = new DynamicXml(
                XmlHelper.StripDashesInElementOrAttributeNames(xml),
                xml);
            var result = dXml.TryConvertTo<RawXmlDocument>();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(xml, result.Result.Value.InnerXml);
        }

        [Test]
        public void Convert_To_String()
        {
            var xml = "<DAMP fullMedia=\"\" test-attribute=\"someValue\"><mediaItem><Image id=\"1057\" version=\"d58d5c16-153e-4896-892f-a722e45a69af\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:55:29\" updateDate=\"2012-11-05T16:55:44\" nodeName=\"test12\" urlName=\"test12\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1057\"><umbracoFile>/media/54/tulips.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>620888</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:55:34\"><crop name=\"thumbCrop\" x=\"154\" y=\"1\" x2=\"922\" y2=\"768\" url=\"/media/54/tulips_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem><mediaItem><Image id=\"1055\" version=\"4df1f08a-3552-45f2-b4bf-fa980c762f4a\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:29:58\" updateDate=\"2012-11-05T16:30:27\" nodeName=\"Test\" urlName=\"test\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1055\"><umbracoFile>/media/41/hydrangeas.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>595284</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:30:18\"><crop name=\"thumbCrop\" x=\"133\" y=\"0\" x2=\"902\" y2=\"768\" url=\"/media/41/hydrangeas_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem></DAMP>";
            var dXml = new DynamicXml(xml);
            var result = dXml.TryConvertTo<string>();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(xml, result.Result);
        }

        [Test]
        public void Convert_To_XElement()
        {
            var xml = "<DAMP fullMedia=\"\" test-attribute=\"someValue\"><mediaItem><Image id=\"1057\" version=\"d58d5c16-153e-4896-892f-a722e45a69af\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:55:29\" updateDate=\"2012-11-05T16:55:44\" nodeName=\"test12\" urlName=\"test12\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1057\"><umbracoFile>/media/54/tulips.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>620888</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:55:34\"><crop name=\"thumbCrop\" x=\"154\" y=\"1\" x2=\"922\" y2=\"768\" url=\"/media/54/tulips_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem><mediaItem><Image id=\"1055\" version=\"4df1f08a-3552-45f2-b4bf-fa980c762f4a\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:29:58\" updateDate=\"2012-11-05T16:30:27\" nodeName=\"Test\" urlName=\"test\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1055\"><umbracoFile>/media/41/hydrangeas.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>595284</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:30:18\"><crop name=\"thumbCrop\" x=\"133\" y=\"0\" x2=\"902\" y2=\"768\" url=\"/media/41/hydrangeas_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem></DAMP>";
            var dXml = new DynamicXml(xml);
            var result = dXml.TryConvertTo<XElement>();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(xml, result.Result.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void Convert_To_XmlElement()
        {
            var xml = "<DAMP fullMedia=\"\" test-attribute=\"someValue\"><mediaItem><Image id=\"1057\" version=\"d58d5c16-153e-4896-892f-a722e45a69af\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:55:29\" updateDate=\"2012-11-05T16:55:44\" nodeName=\"test12\" urlName=\"test12\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1057\"><umbracoFile>/media/54/tulips.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>620888</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:55:34\"><crop name=\"thumbCrop\" x=\"154\" y=\"1\" x2=\"922\" y2=\"768\" url=\"/media/54/tulips_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem><mediaItem><Image id=\"1055\" version=\"4df1f08a-3552-45f2-b4bf-fa980c762f4a\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:29:58\" updateDate=\"2012-11-05T16:30:27\" nodeName=\"Test\" urlName=\"test\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1055\"><umbracoFile>/media/41/hydrangeas.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>595284</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:30:18\"><crop name=\"thumbCrop\" x=\"133\" y=\"0\" x2=\"902\" y2=\"768\" url=\"/media/41/hydrangeas_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem></DAMP>";
            var dXml = new DynamicXml(xml);
            var result = dXml.TryConvertTo<XmlElement>();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(xml, result.Result.OuterXml);
        }

        [Test]
        public void Convert_To_XmlDocument()
        {
            var xml = "<DAMP fullMedia=\"\" test-attribute=\"someValue\"><mediaItem><Image id=\"1057\" version=\"d58d5c16-153e-4896-892f-a722e45a69af\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:55:29\" updateDate=\"2012-11-05T16:55:44\" nodeName=\"test12\" urlName=\"test12\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1057\"><umbracoFile>/media/54/tulips.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>620888</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:55:34\"><crop name=\"thumbCrop\" x=\"154\" y=\"1\" x2=\"922\" y2=\"768\" url=\"/media/54/tulips_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem><mediaItem><Image id=\"1055\" version=\"4df1f08a-3552-45f2-b4bf-fa980c762f4a\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:29:58\" updateDate=\"2012-11-05T16:30:27\" nodeName=\"Test\" urlName=\"test\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1055\"><umbracoFile>/media/41/hydrangeas.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>595284</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:30:18\"><crop name=\"thumbCrop\" x=\"133\" y=\"0\" x2=\"902\" y2=\"768\" url=\"/media/41/hydrangeas_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem></DAMP>";
            var dXml = new DynamicXml(xml);
            var result = dXml.TryConvertTo<XmlDocument>();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(xml, result.Result.InnerXml);
        }
    }
}