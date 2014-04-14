using AutoMapper;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Models.Mapping
{
    [RequiresAutoMapperMappings]
    [TestFixture]
    public class AutoMapperTests : BaseUmbracoApplicationTest
    {
        [Test]
        public void Assert_Valid_Mappings()
        {
            Mapper.AssertConfigurationIsValid();
        }
    }
}