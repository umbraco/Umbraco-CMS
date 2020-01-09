using NUnit.Framework;
using Umbraco.Tests.Shared.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.Models
{
    [TestFixture]
    public class DictionaryItemTests
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var builder = new DictionaryItemBuilder();

            var item = builder
                .Build();
        }
    }
}
