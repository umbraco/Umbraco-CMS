using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class MappingResolverTests : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Can_Map_Id_Property_On_Content()
        {
            var mapping = MappingResolver.GetMapping(typeof (Content), "Id");

            Assert.That(mapping, Is.EqualTo("[umbracoNode].[id]"));
        }

        [Test]
        public void Can_Map_Alias_Property_On_ContentType()
        {
            var mapping = MappingResolver.GetMapping(typeof(ContentType), "Alias");

            Assert.That(mapping, Is.EqualTo("[cmsContentType].[alias]"));
        }

        [Test]
        public void Can_Resolve_ContentType_Mapper()
        {
            var mapper = MappingResolver.ResolveMapperByType(typeof (ContentType));

            Assert.IsTrue(mapper is ContentTypeMapper);
        }
        [Test]
        public void Can_Resolve_Dictionary_Mapper()
        {
            var mapper = MappingResolver.ResolveMapperByType(typeof(IDictionaryItem));

            Assert.IsTrue(mapper is DictionaryMapper);
        }
    }
}