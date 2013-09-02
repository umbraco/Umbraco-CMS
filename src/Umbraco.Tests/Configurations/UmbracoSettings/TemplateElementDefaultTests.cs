using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class TemplateElementDefaultTests : TemplateElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }
    }
}