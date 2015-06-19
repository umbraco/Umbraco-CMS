using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using umbraco;

namespace Umbraco.Tests
{
    [TestFixture]
    public class XmlHelperTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Ignore("This is a benchmark test so is ignored by default")]
        [Test]
        public void Sort_Nodes_Benchmark_Legacy()
        {
            var xmlContent = GetXmlContent(1);
            var xml = new XmlDocument();
            xml.LoadXml(xmlContent);
            var original = xml.GetElementById(1173.ToString());
            Assert.IsNotNull(original);

            long totalTime = 0;
            var watch = new Stopwatch();
            var iterations = 10000;

            for (var i = 0; i < iterations; i++)
            {
                //don't measure the time for clone!
                var parentNode = original.Clone();
                watch.Start();
                LegacySortNodes(ref parentNode);
                watch.Stop();
                totalTime += watch.ElapsedMilliseconds;
                watch.Reset();

                //do assertions just to make sure it is working properly.
                var currSort = 0;
                foreach (var child in parentNode.SelectNodes("./* [@id]").Cast<XmlNode>())
                {
                    Assert.AreEqual(currSort, int.Parse(child.Attributes["sortOrder"].Value));
                    currSort++;
                }

                //ensure the parent node's properties still exist first
                Assert.AreEqual("content", parentNode.ChildNodes[0].Name);
                Assert.AreEqual("umbracoUrlAlias", parentNode.ChildNodes[1].Name);
                //then the child nodes should come straight after
                Assert.IsTrue(parentNode.ChildNodes[2].Attributes["id"] != null);
            }

            Debug.WriteLine("Total time for " + iterations + " iterations is " + totalTime);
        }

        [Ignore("This is a benchmark test so is ignored by default")]
        [Test]
        public void Sort_Nodes_Benchmark_New()
        {
            var xmlContent = GetXmlContent(1);
            var xml = new XmlDocument();
            xml.LoadXml(xmlContent);
            var original = xml.GetElementById(1173.ToString());
            Assert.IsNotNull(original);

            long totalTime = 0;
            var watch = new Stopwatch();
            var iterations = 10000;

            for (var i = 0; i < iterations; i++)
            {
                //don't measure the time for clone!
                var parentNode = (XmlElement)original.Clone();
                watch.Start();
                XmlHelper.SortNodes(
                    parentNode, 
                    "./* [@id]", 
                    x => x.AttributeValue<int>("sortOrder"));
                watch.Stop();
                totalTime += watch.ElapsedMilliseconds;
                watch.Reset();

                //do assertions just to make sure it is working properly.
                var currSort = 0;
                foreach (var child in parentNode.SelectNodes("./* [@id]").Cast<XmlNode>())
                {
                    Assert.AreEqual(currSort, int.Parse(child.Attributes["sortOrder"].Value));
                    currSort++;
                }
                //ensure the parent node's properties still exist first
                Assert.AreEqual("content", parentNode.ChildNodes[0].Name);
                Assert.AreEqual("umbracoUrlAlias", parentNode.ChildNodes[1].Name);
                //then the child nodes should come straight after
                Assert.IsTrue(parentNode.ChildNodes[2].Attributes["id"] != null);

            }

            Debug.WriteLine("Total time for " + iterations + " iterations is " + totalTime);
        }


        [Test]
        public void Sort_Nodes()
        {
            var xmlContent = GetXmlContent(1);
            var xml = new XmlDocument();
            xml.LoadXml(xmlContent);
            var original = xml.GetElementById(1173.ToString());
            Assert.IsNotNull(original);
           
            var parentNode = (XmlElement)original.Clone();
           
            XmlHelper.SortNodes(
                parentNode,
                "./* [@id]",
                x => x.AttributeValue<int>("sortOrder"));
           
            //do assertions just to make sure it is working properly.
            var currSort = 0;
            foreach (var child in parentNode.SelectNodes("./* [@id]").Cast<XmlNode>())
            {
                Assert.AreEqual(currSort, int.Parse(child.Attributes["sortOrder"].Value));
                currSort++;
            }
            //ensure the parent node's properties still exist first
            Assert.AreEqual("content", parentNode.ChildNodes[0].Name);
            Assert.AreEqual("umbracoUrlAlias", parentNode.ChildNodes[1].Name);
            //then the child nodes should come straight after
            Assert.IsTrue(parentNode.ChildNodes[2].Attributes["id"] != null);
        }

        /// <summary>
        /// This was the logic to sort before and now lives here just to show the benchmarks tests above.
        /// </summary>
        /// <param name="parentNode"></param>
        private static void LegacySortNodes(ref XmlNode parentNode)
        {
            XmlNode n = parentNode.CloneNode(true);

            // remove all children from original node
            string xpath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "./node" : "./* [@id]";
            foreach (XmlNode child in parentNode.SelectNodes(xpath))
                parentNode.RemoveChild(child);


            XPathNavigator nav = n.CreateNavigator();
            XPathExpression expr = nav.Compile(xpath);
            expr.AddSort("@sortOrder", XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Number);
            XPathNodeIterator iterator = nav.Select(expr);
            while (iterator.MoveNext())
                parentNode.AppendChild(
                    ((IHasXmlNode)iterator.Current).GetNode());
        }

        /// <summary>
        /// returns xml with a reverse sort order
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        private string GetXmlContent(int templateId)
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[ 
<!ELEMENT Home ANY>
<!ATTLIST Home id ID #REQUIRED>
<!ELEMENT CustomDocument ANY>
<!ATTLIST CustomDocument id ID #REQUIRED>
]>
<root id=""-1"">
	<Home id=""1046"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""home"" writerName=""admin"" creatorName=""admin"" path=""-1,1046"" isDoc="""">
		<content><![CDATA[]]></content>
		<umbracoUrlAlias><![CDATA[this/is/my/alias, anotheralias]]></umbracoUrlAlias>
		<umbracoNaviHide>1</umbracoNaviHide>
		<Home id=""1173"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""sub1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173"" isDoc="""">
			<content><![CDATA[<div>This is some content</div>]]></content>
			<umbracoUrlAlias><![CDATA[page2/alias, 2ndpagealias]]></umbracoUrlAlias>			
			<Home id=""1174"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""3"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""sub2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1174"" isDoc="""">
				<content><![CDATA[]]></content>
				<umbracoUrlAlias><![CDATA[only/one/alias]]></umbracoUrlAlias>
				<creatorName><![CDATA[Custom data with same property name as the member name]]></creatorName>
			</Home>
			<Home id=""1176"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
			<CustomDocument id=""1177"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""custom sub 1"" urlName=""custom-sub-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1177"" isDoc="""" />
			<CustomDocument id=""1178"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-16T14:23:35"" nodeName=""custom sub 2"" urlName=""custom-sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1178"" isDoc="""" />
            
            <Home id=""1176"" parentID=""1179"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""26"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1180"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""25"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1181"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""24"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1182"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""23"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1183"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""22"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1184"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""21"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1185"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""20"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1186"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""19"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1187"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""18"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1188"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""17"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1189"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""16"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1190"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""15"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1191"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""14"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1192"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""13"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1193"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""12"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1194"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""11"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1195"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""10"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1196"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""9"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1197"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""8"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1198"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""7"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1199"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""6"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1200"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""5"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
            <Home id=""1176"" parentID=""1201"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""4"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
		</Home>
		<Home id=""1175"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1175"" isDoc=""""><content><![CDATA[]]></content>
		</Home>
	</Home>
	<CustomDocument id=""1172"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test-page"" writerName=""admin"" creatorName=""admin"" path=""-1,1172"" isDoc="""" />
</root>";
        }

    }
}
