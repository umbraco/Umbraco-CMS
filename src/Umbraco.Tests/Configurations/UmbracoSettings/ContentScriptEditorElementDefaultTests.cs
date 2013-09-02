using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ContentScriptEditorElementDefaultTests : ContentScriptEditorElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }
    }
}