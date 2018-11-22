using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ScriptingElementDefaultTests : ScriptingElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

        [Test]
        public override void DataTypeModelStaticMappings()
        {
            Assert.AreEqual(0, SettingsSection.Scripting.DataTypeModelStaticMappings.Count());
        }

    }
}