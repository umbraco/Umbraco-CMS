using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using umbraco;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using LightInject;
using Moq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Misc
{

    /// <summary>
    /// Tests for the legacy library class
    /// </summary>
    [TestFixture]
    public class LibraryTests : BaseWebTest
    {
        public override void SetUp()
        {
            base.SetUp();

            var dataTypeService = new TestObjects.TestDataTypeService(
                new DataType(new VoidEditor(Mock.Of<ILogger>())) { Id = 1 });

            var factory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), new PropertyValueConverterCollection(Array.Empty<IPropertyValueConverter>()), dataTypeService);

            // need to specify a custom callback for unit tests
            // AutoPublishedContentTypes generates properties automatically
            // when they are requested, but we must declare those that we
            // explicitely want to be here...

            var propertyTypes = new[]
                {
                    // AutoPublishedContentType will auto-generate other properties
                    factory.CreatePropertyType("content", 1),
                };
            var type = new AutoPublishedContentType(0, "anything", propertyTypes);
            ContentTypesCache.GetPublishedContentTypeByAlias = (alias) => type;
            Debug.Print("INIT LIB {0}",
                ContentTypesCache.Get(PublishedItemType.Content, "anything")
                    .PropertyTypes.Count());

            var umbracoContext = GetUmbracoContext("/test");
            Umbraco.Web.Composing.Current.UmbracoContextAccessor.UmbracoContext = umbracoContext;
        }

        /// <summary>
        /// sets up resolvers before resolution is frozen
        /// </summary>
        protected override void Compose()
        {
            base.Compose();

            // required so we can access property.Value
            if (Container.TryGetInstance<PropertyValueConverterCollectionBuilder>() == null)
                Container.RegisterCollectionBuilder<PropertyValueConverterCollectionBuilder>();
        }


        /// <summary>
        /// The old method, just using this to make sure we're returning the correct exact data as before.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        private string LegacyGetItem(int nodeId, string alias)
        {
            var cache = UmbracoContext.Current.ContentCache as PublishedContentCache;
            if (cache == null) throw new Exception("Unsupported IPublishedContentCache, only the Xml one is supported.");
            var umbracoXML = cache.GetXml(UmbracoContext.Current.InPreviewMode);

            string xpath = "./{0}";
            if (umbracoXML.GetElementById(nodeId.ToString()) != null)
                if (
                    ",id,parentID,level,writerID,template,sortOrder,createDate,updateDate,nodeName,writerName,path,"
                        .
                        IndexOf("," + alias + ",") > -1)
                    return umbracoXML.GetElementById(nodeId.ToString()).Attributes.GetNamedItem(alias).Value;
                else if (
                    umbracoXML.GetElementById(nodeId.ToString()).SelectSingleNode(string.Format(xpath, alias)) !=
                    null)
                    return
                        umbracoXML.GetElementById(nodeId.ToString()).SelectSingleNode(string.Format(xpath, alias)).ChildNodes[0].
                            Value; //.Value + "*";
                else
                    return string.Empty;
            else
                return string.Empty;
        }
    }
}
