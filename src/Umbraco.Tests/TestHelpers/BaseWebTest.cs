using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Core.Serialization;
using Umbraco.Extensions;
using Umbraco.Tests.Common;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web.Composing;

namespace Umbraco.Tests.TestHelpers
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public abstract class BaseWebTest : TestWithDatabaseBase
    {
        protected override void Compose()
        {
            base.Compose();

            Builder.Services.AddUnique<IPublishedValueFallback, PublishedValueFallback>();
            Builder.Services.AddUnique<IProfilingLogger, ProfilingLogger>();
        }

        protected override void Initialize()
        {
            base.Initialize();

            // need to specify a custom callback for unit tests
            // AutoPublishedContentTypes generates properties automatically

            var serializer = new ConfigurationEditorJsonSerializer();
            var dataTypeService = new TestObjects.TestDataTypeService(
                new DataType(new VoidEditor(NullLoggerFactory.Instance, Mock.Of<IDataTypeService>(), Mock.Of<ILocalizationService>(),Mock.Of<ILocalizedTextService>(), Mock.Of<IShortStringHelper>(), JsonNetSerializer), serializer) { Id = 1 });

            var factory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), new PropertyValueConverterCollection(Array.Empty<IPropertyValueConverter>()), dataTypeService);
            var type = new AutoPublishedContentType(Guid.NewGuid(), 0, "anything", new PublishedPropertyType[] { });
            ContentTypesCache.GetPublishedContentTypeByAlias = alias => GetPublishedContentTypeByAlias(alias) ?? type;
        }

        protected virtual PublishedContentType GetPublishedContentTypeByAlias(string alias) => null;

        protected override string GetXmlContent(int templateId)
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[
<!ELEMENT Home ANY>
<!ATTLIST Home id ID #REQUIRED>
<!ELEMENT CustomDocument ANY>
<!ATTLIST CustomDocument id ID #REQUIRED>
]>
<root id=""-1"">
    <Home id=""1046"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""home"" writerName=""admin"" creatorName=""admin"" path=""-1,1046"" isDoc="""">
        <content><![CDATA[]]></content>
        <umbracoUrlAlias><![CDATA[this/is/my/alias, anotheralias]]></umbracoUrlAlias>
        <umbracoNaviHide>1</umbracoNaviHide>
        <Home id=""1173"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""sub1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173"" isDoc="""">
            <content><![CDATA[<div>This is some content</div>]]></content>
            <umbracoUrlAlias><![CDATA[page2/alias, 2ndpagealias]]></umbracoUrlAlias>
            <Home id=""1174"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""sub2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1174"" isDoc="""">
                <content><![CDATA[]]></content>
                <umbracoUrlAlias><![CDATA[only/one/alias]]></umbracoUrlAlias>
                <creatorName><![CDATA[Custom data with same property name as the member name]]></creatorName>
            </Home>
            <Home id=""1176"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""3"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
                <content><![CDATA[]]></content>
            </Home>
            <CustomDocument id=""1177"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""4"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""custom sub 1"" urlName=""custom-sub-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1177"" isDoc="""" />
            <CustomDocument id=""1178"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""4"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-16T14:23:35"" nodeName=""custom sub 2"" urlName=""custom-sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1178"" isDoc="""" />
            <CustomDocument id=""1179"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""4"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-16T14:23:35"" nodeName=""custom sub 3 with accént character"" urlName=""custom-sub-3-with-accént-character"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1179"" isDoc="""" />
            <CustomDocument id=""1180"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""4"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-16T14:23:35"" nodeName=""custom sub 4 with æøå"" urlName=""custom-sub-4-with-æøå"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1180"" isDoc="""" />
        </Home>
        <Home id=""1175"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""3"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1175"" isDoc=""""><content><![CDATA[]]></content>
        </Home>
    </Home>
    <CustomDocument id=""1172"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test-page"" writerName=""admin"" creatorName=""admin"" path=""-1,1172"" isDoc="""" />
</root>";
        }

        internal PublishedRouter CreatePublishedRouter(IUmbracoContextAccessor umbracoContextAccessor, IServiceProvider container = null, ContentFinderCollection contentFinders = null)
        {
            var webRoutingSettings = new WebRoutingSettings();
            return CreatePublishedRouter(umbracoContextAccessor, webRoutingSettings, container ?? Factory, contentFinders);
        }

        internal static PublishedRouter CreatePublishedRouter(IUmbracoContextAccessor umbracoContextAccessor, WebRoutingSettings webRoutingSettings, IServiceProvider container = null, ContentFinderCollection contentFinders = null)
            => new PublishedRouter(
                Microsoft.Extensions.Options.Options.Create(webRoutingSettings),
                contentFinders ?? new ContentFinderCollection(Enumerable.Empty<IContentFinder>()),
                new TestLastChanceFinder(),
                new TestVariationContextAccessor(),
                new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>()),
                Mock.Of<ILogger<PublishedRouter>>(),
                Mock.Of<IPublishedUrlProvider>(),
                Mock.Of<IRequestAccessor>(),
                container?.GetRequiredService<IPublishedValueFallback>() ?? Current.Factory.GetRequiredService<IPublishedValueFallback>(),
                container?.GetRequiredService<IPublicAccessChecker>() ?? Current.Factory.GetRequiredService<IPublicAccessChecker>(),
                container?.GetRequiredService<IFileService>() ?? Current.Factory.GetRequiredService<IFileService>(),
                container?.GetRequiredService<IContentTypeService>() ?? Current.Factory.GetRequiredService<IContentTypeService>(),
                container?.GetRequiredService<IPublicAccessService>() ?? Current.Factory.GetRequiredService<IPublicAccessService>(),
                umbracoContextAccessor,
                Mock.Of<IEventAggregator>()
            );
    }
}
