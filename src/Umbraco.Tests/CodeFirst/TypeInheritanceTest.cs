using System.Linq;
using NUnit.Framework;
using Umbraco.Tests.CodeFirst.TestModels.Composition;

namespace Umbraco.Tests.CodeFirst
{
    [TestFixture]
    public class TypeInheritanceTest
    {
        [Test]
        public void Can_Get_Interfaces_From_Type()
        {
            var type = typeof (News);
            var interfaces = type.GetInterfaces().ToList();

            bool hasSeo = interfaces.Any(x => x.Name == typeof(ISeo).Name);
            bool hasMeta = interfaces.Any(x => x.Name == typeof(IMeta).Name);

            Assert.That(hasSeo, Is.True);
            Assert.That(hasMeta, Is.True);
            Assert.That(interfaces.Count, Is.EqualTo(3));
        }
    }
}