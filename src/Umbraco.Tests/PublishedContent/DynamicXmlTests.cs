using System;
using System.Diagnostics;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using System.Linq;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
	public class DynamicXmlTests
	{
        /// <summary>
        /// Ensures that when we return the xml structure we get the real structure, not the replaced hyphen structure
        /// see: http://issues.umbraco.org/issue/U4-1405#comment=67-5113
        ///         http://issues.umbraco.org/issue/U4-1636
        /// </summary>
        [Test]
        public void Deals_With_Hyphenated_Values()
        {
            var xml = @"<banner1Link>
  <url-picker some-attribute='Content'>
    <new-window>True</new-window>
    <node-id>1161</node-id>
    <url>/content/</url>
    <link-title>12 december Zorgbeurs Care</link-title>
  </url-picker>
</banner1Link>";

            var typedXml = new DynamicXml(
                XmlHelper.StripDashesInElementOrAttributeNames(xml),
                xml);
            dynamic dynamicXml = typedXml;

            var typedElement = typedXml.RawXmlElement.Element("url-picker");
            var dynamicElementByCleanedName = dynamicXml.urlpicker;

            Assert.IsNotNull(typedElement);
            Assert.IsNotNull(dynamicElementByCleanedName);

            Assert.AreEqual(
                typedElement.Attribute("some-attribute").Value,
                dynamicElementByCleanedName.someattribute);            
        }

        [Test]
		public void Custom_Extension_Method_Legacy()
		{
			var xml = "<DAMP fullMedia=\"\"><mediaItem><Image id=\"1057\" version=\"d58d5c16-153e-4896-892f-a722e45a69af\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:55:29\" updateDate=\"2012-11-05T16:55:44\" nodeName=\"test12\" urlName=\"test12\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1057\"><umbracoFile>/media/54/tulips.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>620888</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:55:34\"><crop name=\"thumbCrop\" x=\"154\" y=\"1\" x2=\"922\" y2=\"768\" url=\"/media/54/tulips_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem><mediaItem><Image id=\"1055\" version=\"4df1f08a-3552-45f2-b4bf-fa980c762f4a\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:29:58\" updateDate=\"2012-11-05T16:30:27\" nodeName=\"Test\" urlName=\"test\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1055\"><umbracoFile>/media/41/hydrangeas.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>595284</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:30:18\"><crop name=\"thumbCrop\" x=\"133\" y=\"0\" x2=\"902\" y2=\"768\" url=\"/media/41/hydrangeas_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem></DAMP>";
			var typedXml = new global::umbraco.MacroEngines.DynamicXml(xml);
			dynamic dynamicXml = typedXml;

			//we haven't explicitly defined ElementAt so this will dynamically invoke this method
			var element = dynamicXml.ElementAt(0);

			Assert.AreEqual("1057", Enumerable.First(element.BaseElement.Elements()).Attribute("id").Value);
		}

		[Test]
		public void Custom_Extension_Method()
		{
			var xml = "<DAMP fullMedia=\"\"><mediaItem><Image id=\"1057\" version=\"d58d5c16-153e-4896-892f-a722e45a69af\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:55:29\" updateDate=\"2012-11-05T16:55:44\" nodeName=\"test12\" urlName=\"test12\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1057\"><umbracoFile>/media/54/tulips.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>620888</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:55:34\"><crop name=\"thumbCrop\" x=\"154\" y=\"1\" x2=\"922\" y2=\"768\" url=\"/media/54/tulips_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem><mediaItem><Image id=\"1055\" version=\"4df1f08a-3552-45f2-b4bf-fa980c762f4a\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:29:58\" updateDate=\"2012-11-05T16:30:27\" nodeName=\"Test\" urlName=\"test\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1055\"><umbracoFile>/media/41/hydrangeas.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>595284</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:30:18\"><crop name=\"thumbCrop\" x=\"133\" y=\"0\" x2=\"902\" y2=\"768\" url=\"/media/41/hydrangeas_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem></DAMP>";
			var typedXml = new DynamicXml(xml);
			
			dynamic dynamicXml = typedXml;

			//we haven't explicitly defined ElementAt so this will dynamically invoke this method
			var element = dynamicXml.ElementAt(0);

            Assert.AreEqual("1057", Enumerable.First(element.BaseElement.Elements()).Attribute("id").Value);
		}

		[Test]
		public void Take_Legacy()
		{
			var xml = "<DAMP fullMedia=\"\"><mediaItem><Image id=\"1057\" version=\"d58d5c16-153e-4896-892f-a722e45a69af\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:55:29\" updateDate=\"2012-11-05T16:55:44\" nodeName=\"test12\" urlName=\"test12\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1057\"><umbracoFile>/media/54/tulips.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>620888</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:55:34\"><crop name=\"thumbCrop\" x=\"154\" y=\"1\" x2=\"922\" y2=\"768\" url=\"/media/54/tulips_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem><mediaItem><Image id=\"1055\" version=\"4df1f08a-3552-45f2-b4bf-fa980c762f4a\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:29:58\" updateDate=\"2012-11-05T16:30:27\" nodeName=\"Test\" urlName=\"test\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1055\"><umbracoFile>/media/41/hydrangeas.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>595284</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:30:18\"><crop name=\"thumbCrop\" x=\"133\" y=\"0\" x2=\"902\" y2=\"768\" url=\"/media/41/hydrangeas_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem></DAMP>";
			var typedXml = new global::umbraco.MacroEngines.DynamicXml(xml);
			dynamic dynamicXml = typedXml;
			var typedTaken = typedXml.Take(1);
			var dynamicTaken = dynamicXml.Take(1);

			Assert.AreEqual(1, typedTaken.Count());
			Assert.AreEqual(1, Enumerable.Count(dynamicTaken));			

			Assert.AreEqual("1057", typedTaken.ElementAt(0).BaseElement.Elements().First().Attribute("id").Value);
			Assert.AreEqual("1057", Enumerable.First(Enumerable.ElementAt(dynamicTaken, 0).BaseElement.Elements()).Attribute("id").Value);
		}

		[Test]
		public void Take()
		{
			var xml = "<DAMP fullMedia=\"\"><mediaItem><Image id=\"1057\" version=\"d58d5c16-153e-4896-892f-a722e45a69af\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:55:29\" updateDate=\"2012-11-05T16:55:44\" nodeName=\"test12\" urlName=\"test12\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1057\"><umbracoFile>/media/54/tulips.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>620888</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:55:34\"><crop name=\"thumbCrop\" x=\"154\" y=\"1\" x2=\"922\" y2=\"768\" url=\"/media/54/tulips_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem><mediaItem><Image id=\"1055\" version=\"4df1f08a-3552-45f2-b4bf-fa980c762f4a\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:29:58\" updateDate=\"2012-11-05T16:30:27\" nodeName=\"Test\" urlName=\"test\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1055\"><umbracoFile>/media/41/hydrangeas.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>595284</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:30:18\"><crop name=\"thumbCrop\" x=\"133\" y=\"0\" x2=\"902\" y2=\"768\" url=\"/media/41/hydrangeas_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem></DAMP>";
			var typedXml = new DynamicXml(xml);
			dynamic dynamicXml = typedXml;
			var typedTaken = typedXml.Take(1);
			var dynamicTaken = dynamicXml.Take(1);

			Assert.AreEqual(1, typedTaken.Count());
			Assert.AreEqual(1, Enumerable.Count(dynamicTaken));

            Assert.AreEqual("1057", typedTaken.ElementAt(0).BaseElement.Elements().First().Attribute("id").Value);
            Assert.AreEqual("1057", Enumerable.First(Enumerable.ElementAt(dynamicTaken, 0).BaseElement.Elements()).Attribute("id").Value);
		}

		[Test]
		public void Ensure_Legacy_Objects_Are_Returned()
		{
			var xml = "<DAMP fullMedia=\"\"><mediaItem><Image id=\"1057\" version=\"d58d5c16-153e-4896-892f-a722e45a69af\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:55:29\" updateDate=\"2012-11-05T16:55:44\" nodeName=\"test12\" urlName=\"test12\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1057\"><umbracoFile>/media/54/tulips.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>620888</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:55:34\"><crop name=\"thumbCrop\" x=\"154\" y=\"1\" x2=\"922\" y2=\"768\" url=\"/media/54/tulips_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem><mediaItem><Image id=\"1055\" version=\"4df1f08a-3552-45f2-b4bf-fa980c762f4a\" parentID=\"-1\" level=\"1\" writerID=\"0\" nodeType=\"1032\" template=\"0\" sortOrder=\"1\" createDate=\"2012-11-05T16:29:58\" updateDate=\"2012-11-05T16:30:27\" nodeName=\"Test\" urlName=\"test\" writerName=\"admin\" nodeTypeAlias=\"Image\" path=\"-1,1055\"><umbracoFile>/media/41/hydrangeas.jpg</umbracoFile><umbracoWidth>1024</umbracoWidth><umbracoHeight>768</umbracoHeight><umbracoBytes>595284</umbracoBytes><umbracoExtension>jpg</umbracoExtension><newsCrops><crops date=\"2012-11-05T16:30:18\"><crop name=\"thumbCrop\" x=\"133\" y=\"0\" x2=\"902\" y2=\"768\" url=\"/media/41/hydrangeas_thumbCrop.jpg\" /></crops></newsCrops></Image></mediaItem></DAMP>";
			var mediaItems = new global::umbraco.MacroEngines.DynamicXml(xml);
			//Debug.WriteLine("full xml = {0}", mediaItems.ToXml());

			if (mediaItems.Count() != 0)
			{
				foreach (dynamic item in mediaItems)
				{
					Type itemType = item.GetType();
					Debug.WriteLine("item type = {0}", itemType);
					dynamic image = item.Image;

					Type imageType = image.GetType();
					Debug.WriteLine("image type = {0}", imageType);
					
					//ensure they are the same
					Assert.AreEqual(itemType, imageType);

					//ensure they are legacy
					Assert.AreEqual(typeof(global::umbraco.MacroEngines.DynamicXml), itemType);
					Assert.AreEqual(typeof(global::umbraco.MacroEngines.DynamicXml), imageType);
				}
			}
		}

		/// <summary>
		/// Test the current Core class
		/// </summary>
		[Test]
		public void Find_Test_Core_Class()
		{
			RunFindTest(x => new DynamicXml(x));
		}

		/// <summary>
		/// Tests the macroEngines legacy class
		/// </summary>
		[Test]
		public void Find_Test_Legacy_Class()
		{
			RunFindTest(x => new global::umbraco.MacroEngines.DynamicXml(x));
		}

		private void RunFindTest(Func<string, dynamic> getDynamicXml)
		{
			var xmlstring = @"<test>
<item id='1' name='test 1' value='found 1'/>
<item id='2' name='test 2' value='found 2'/>
<item id='3' name='test 3' value='found 3'/>
</test>";

			dynamic dXml = getDynamicXml(xmlstring);

			var result1 = dXml.Find("@name", "test 1");
			var result2 = dXml.Find("@name", "test 2");
			var result3 = dXml.Find("@name", "test 3");
			var result4 = dXml.Find("@name", "dont find");

			Assert.AreEqual("found 1", result1.value);
			Assert.AreEqual("found 2", result2.value);
			Assert.AreEqual("found 3", result3.value);
			Assert.Throws<RuntimeBinderException>(() =>
			{
				//this will throw because result4 is not found
				var temp = result4.value;
			});
		}

	}
}