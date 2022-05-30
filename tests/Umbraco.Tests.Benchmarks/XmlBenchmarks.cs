using System;
using System.Xml;
using BenchmarkDotNet.Attributes;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

[QuickRunWithMemoryDiagnoserConfig]
public class XmlBenchmarks
{
    private const bool UseLegacySchema = false;

    private XmlDocument _xml;

    [GlobalSetup]
    public void Setup()
    {
        var templateId = 0;
        var xmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[
<!ELEMENT Home ANY>
<!ATTLIST Home id ID #REQUIRED>
<!ELEMENT CustomDocument ANY>
<!ATTLIST CustomDocument id ID #REQUIRED>
]>
<root id=""-1"">
    <Home id=""1046"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
                      templateId +
                      @""" sortOrder=""1"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""home"" writerName=""admin"" creatorName=""admin"" path=""-1,1046"" isDoc="""">
        <content><![CDATA[]]></content>
        <umbracoUrlAlias><![CDATA[this/is/my/alias, anotheralias]]></umbracoUrlAlias>
        <umbracoNaviHide>1</umbracoNaviHide>
        <Home id=""1173"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
                      templateId +
                      @""" sortOrder=""2"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""sub1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173"" isDoc="""">
            <content><![CDATA[<div>This is some content</div>]]></content>
            <umbracoUrlAlias><![CDATA[page2/alias, 2ndpagealias]]></umbracoUrlAlias>
            <Home id=""1174"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
                      templateId +
                      @""" sortOrder=""2"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""sub2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1174"" isDoc="""">
                <content><![CDATA[]]></content>
                <umbracoUrlAlias><![CDATA[only/one/alias]]></umbracoUrlAlias>
                <creatorName><![CDATA[Custom data with same property name as the member name]]></creatorName>
            </Home>
            <Home id=""1176"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
                      templateId +
                      @""" sortOrder=""3"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                <content><![CDATA[]]></content>
            </Home>
            <CustomDocument id=""1177"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" +
                      templateId +
                      @""" sortOrder=""4"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""custom sub 1"" urlName=""custom-sub-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1177"" isDoc="""" />
            <CustomDocument id=""1178"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" +
                      templateId +
                      @""" sortOrder=""4"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-16T14:23:35"" nodeName=""custom sub 2"" urlName=""custom-sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1178"" isDoc="""" />
        </Home>
        <Home id=""1175"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
                      templateId +
                      @""" sortOrder=""3"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1175"" isDoc=""""><content><![CDATA[]]></content>
        </Home>
    </Home>
    <CustomDocument id=""1172"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" +
                      templateId +
                      @""" sortOrder=""2"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test-page"" writerName=""admin"" creatorName=""admin"" path=""-1,1172"" isDoc="""" />
</root>";
        _xml = new XmlDocument();
        _xml.LoadXml(xmlText);
    }

    [GlobalCleanup]
    public void Cleanup() => _xml = null;

    [Benchmark]
    public void XmlWithXPath()
    {
        var xpath =
            "/root/* [@isDoc and @urlName='home']//* [@isDoc and @urlName='sub1']//* [@isDoc and @urlName='sub2']";
        var elt = _xml.SelectSingleNode(xpath);
        if (elt == null)
        {
            Console.WriteLine("ERR");
        }
    }

    [Benchmark]
    public void XmlWithNavigation()
    {
        var elt = _xml.DocumentElement;
        var id = NavigateElementRoute(elt, new[] { "home", "sub1", "sub2" });
        if (id <= 0)
        {
            Console.WriteLine("ERR");
        }
    }

    private int NavigateElementRoute(XmlElement elt, string[] urlParts)
    {
        var found = true;
        var i = 0;
        while (found && i < urlParts.Length)
        {
            found = false;
            foreach (XmlElement child in elt.ChildNodes)
            {
                var noNode = UseLegacySchema
                    ? child.Name != "node"
                    : child.GetAttributeNode("isDoc") == null;
                if (noNode)
                {
                    continue;
                }

                if (child.GetAttribute("urlName") != urlParts[i])
                {
                    continue;
                }

                found = true;
                elt = child;
                break;
            }

            i++;
        }

        return found ? int.Parse(elt.GetAttribute("id")) : -1;
    }
}
