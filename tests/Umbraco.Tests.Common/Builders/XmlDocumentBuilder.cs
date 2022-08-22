// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Xml;

namespace Umbraco.Cms.Tests.Common.Builders;

public class XmlDocumentBuilder : BuilderBase<XmlDocument>
{
    private const string DefaultContent =
        @"<?xml version=""1.0"" encoding=""utf-8""?>
            <!DOCTYPE root[
            <!ELEMENT Home ANY>
            <!ATTLIST Home id ID #REQUIRED>
            <!ELEMENT CustomDocument ANY>
            <!ATTLIST CustomDocument id ID #REQUIRED>
            ]>
            <root id=""-1"">
                <Home id=""1046"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""0"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""home"" writerName=""admin"" creatorName=""admin"" path=""-1,1046"" isDoc="""">
                    <content><![CDATA[]]></content>
                    <umbracoUrlAlias><![CDATA[this/is/my/alias, anotheralias]]></umbracoUrlAlias>
                    <umbracoNaviHide>1</umbracoNaviHide>
                    <Home id=""1173"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""0"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""sub1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173"" isDoc="""">
                        <content><![CDATA[<div>This is some content</div>]]></content>
                        <umbracoUrlAlias><![CDATA[page2/alias, 2ndpagealias]]></umbracoUrlAlias>
                        <Home id=""1174"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""3"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""sub2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1174"" isDoc="""">
                            <content><![CDATA[]]></content>
                            <umbracoUrlAlias><![CDATA[only/one/alias]]></umbracoUrlAlias>
                            <creatorName><![CDATA[Custom data with same property name as the member name]]></creatorName>
                        </Home>
                        <Home id=""1176"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <CustomDocument id=""1177"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""1"" sortOrder=""1"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""custom sub 1"" urlName=""custom-sub-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1177"" isDoc="""" />
                        <CustomDocument id=""1178"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""1"" sortOrder=""0"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-16T14:23:35"" nodeName=""custom sub 2"" urlName=""custom-sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1178"" isDoc="""" />

                        <Home id=""1176"" parentID=""1179"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""26"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1180"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""25"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1181"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""24"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1182"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""23"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1183"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""22"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1184"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""21"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1185"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""20"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1186"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""19"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1187"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""18"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1188"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""17"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1189"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""16"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1190"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""15"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1191"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""14"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1192"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""13"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1193"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""12"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1194"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""11"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1195"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""10"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1196"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""9"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1197"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""8"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1198"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""7"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1199"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""6"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1200"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""5"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                        <Home id=""1176"" parentID=""1201"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""4"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                            <content><![CDATA[]]></content>
                        </Home>
                    </Home>
                    <Home id=""1175"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""1"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1175"" isDoc=""""><content><![CDATA[]]></content>
                    </Home>
                </Home>
                <CustomDocument id=""1172"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""1   "" sortOrder=""1"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test-page"" writerName=""admin"" creatorName=""admin"" path=""-1,1172"" isDoc="""" />
            </root>";

    private string _content;

    public XmlDocumentBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public override XmlDocument Build()
    {
        var xml = new XmlDocument();
        xml.LoadXml(_content ?? DefaultContent);
        return xml;
    }
}
