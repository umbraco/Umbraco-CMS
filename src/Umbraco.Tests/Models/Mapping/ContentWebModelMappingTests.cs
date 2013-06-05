using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Tests.Models.Mapping
{
    [TestFixture]
    public class ContentWebModelMappingTests : BaseDatabaseFactoryTest
    {
        [PropertyEditor("00000000-0000-0000-0000-000000000000", "Test", "~/Test.html")]
        public class TestPropertyEditor : PropertyEditor
        {
            
        }

        protected override void FreezeResolution()
        {
            PropertyEditorResolver.Current = new PropertyEditorResolver(
                () => new List<Type> {typeof (TestPropertyEditor)});

            base.FreezeResolution();
        }

        [Test]
        public void To_Display_Model()
        {
            var contentType = MockedContentTypes.CreateSimpleContentType();
            var content = MockedContent.CreateSimpleContent(contentType);

            var mapper = new ContentModelMapper(ApplicationContext, new ProfileModelMapper());

            var result = mapper.ToContentItemDisplay(content);

            Assert.AreEqual(content.Name, result.Name);
            Assert.AreEqual(content.Id, result.Id);
            Assert.AreEqual(content.Properties.Count(), result.Properties.Count());
            Assert.AreEqual(content.PropertyGroups.Count(), result.Tabs.Count() - 1);
            Assert.IsTrue(result.Tabs.Any(x => x.Label == "Generic properties"));
        }

        [Test]
        public void To_Display_Model_With_Non_Grouped_Properties()
        {
            var idSeed = 1;
            var contentType = MockedContentTypes.CreateSimpleContentType();            
            //add non-grouped properties
            contentType.AddPropertyType(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "nonGrouped1", Name = "Non Grouped 1", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentType.AddPropertyType(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "nonGrouped2", Name = "Non Grouped 2", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            //set ids or it wont work
            contentType.Id = idSeed;
            foreach (var p in contentType.PropertyTypes)
            {
                p.Id = idSeed;
                idSeed++;
            }
            var content = MockedContent.CreateSimpleContent(contentType);
            foreach (var p in content.Properties)
            {
                p.Id = idSeed;
                idSeed++;
            }
            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);
            //ensure that nothing is marked as dirty
            content.ResetDirtyProperties(false);

            var mapper = new ContentModelMapper(ApplicationContext, new ProfileModelMapper());

            var result = mapper.ToContentItemDisplay(content);

            Assert.AreEqual(content.Name, result.Name);
            Assert.AreEqual(content.Id, result.Id);
            Assert.AreEqual(content.Properties.Count(), result.Properties.Count());
            Assert.AreEqual(content.PropertyGroups.Count(), result.Tabs.Count() - 1);
            Assert.IsTrue(result.Tabs.Any(x => x.Label == "Generic properties"));
            Assert.AreEqual(2, result.Tabs.Where(x => x.Label == "Generic properties").SelectMany(x => x.Properties).Count());
        }
    }
}
